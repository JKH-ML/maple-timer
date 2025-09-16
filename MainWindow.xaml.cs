using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace 메이플_타이머
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private int totalSeconds;
        private int currentSeconds;
        private bool isRunning = false;
        private Settings settings;
        private MediaPlayer mediaPlayer;
        private int lastMinute = -1;

        public MainWindow()
        {
            InitializeComponent();
            settings = Settings.Load();
            mediaPlayer = new MediaPlayer();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            LoadSettings();
            LoadWindowPosition();
            ResetTimer();
        }

        private void LoadSettings()
        {
            totalSeconds = settings.DefaultTimerMinutes * 60;
            currentSeconds = totalSeconds;
            MasterVolumeSlider.Value = settings.MasterVolume;
            VolumeLabel.Text = $"{settings.MasterVolume}%";
            UpdateDisplay();
        }

        private void LoadWindowPosition()
        {
            if (!double.IsNaN(settings.WindowLeft) && !double.IsNaN(settings.WindowTop))
            {
                // 화면 범위 내에 있는지 확인
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                if (settings.WindowLeft >= 0 && settings.WindowLeft < screenWidth - this.Width &&
                    settings.WindowTop >= 0 && settings.WindowTop < screenHeight - this.Height)
                {
                    this.WindowStartupLocation = WindowStartupLocation.Manual;
                    this.Left = settings.WindowLeft;
                    this.Top = settings.WindowTop;
                }
            }
        }

        private void SaveWindowPosition()
        {
            settings.WindowLeft = this.Left;
            settings.WindowTop = this.Top;
            settings.Save();
        }

        private void UpdateDisplay()
        {
            int hours = currentSeconds / 3600;
            int minutes = (currentSeconds % 3600) / 60;
            int seconds = currentSeconds % 60;

            // 1. 전체 시간 표시
            if (hours > 0)
                TotalTimeDisplay.Text = $"{hours}:{minutes:D2}:{seconds:D2}";
            else
                TotalTimeDisplay.Text = $"{minutes:D2}:{seconds:D2}";

            // 2. 30분까지 남은 시간 (30분 단위로 리셋)
            int remainingFor30Min = currentSeconds % 1800; // 1800초 = 30분
            int min30 = remainingFor30Min / 60;
            int sec30 = remainingFor30Min % 60;
            ThirtyMinRemainDisplay.Text = $"{min30:D2}:{sec30:D2}";

            // 3. 1분까지 남은 시간 (1분 단위로 리셋)
            int remainingFor1Min = currentSeconds % 60; // 60초 = 1분
            OneMinRemainDisplay.Text = $"{remainingFor1Min:D2}";

            // 프로그레스바 업데이트
            if (totalSeconds > 0)
            {
                double progress = (double)(totalSeconds - currentSeconds) / totalSeconds * 100;
                TimerProgressBar.Value = progress;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentSeconds > 0)
            {
                currentSeconds--;
                UpdateDisplay();

                int currentMinute = currentSeconds / 60;
                int remainingMinutes = currentMinute;

                if (currentSeconds > 0)
                {
                    if (remainingMinutes % 30 == 0 && remainingMinutes != lastMinute && remainingMinutes > 0)
                    {
                        PlaySpecialNotification();
                        lastMinute = currentMinute;
                    }
                    else if (currentSeconds % 60 == 0 && remainingMinutes % 30 != 0)
                    {
                        PlayBasicNotification();
                    }
                }
            }
            else
            {
                timer.Stop();
                isRunning = false;
                PlayStopButton.Content = "▶";
                StatusText.Text = "완료";
                PlayEndNotification();
            }
        }

        private void PlayBasicNotification()
        {
            if (settings.BasicNotificationVolume > 0)
            {
                PlaySound("src/1-min-custom.mp3", "src/1-min.mp3", settings.BasicNotificationVolume);
            }
        }

        private void PlaySpecialNotification()
        {
            if (settings.SpecialNotificationVolume > 0)
            {
                PlaySound("src/30-min-custom.mp3", "src/30-min.mp3", settings.SpecialNotificationVolume);
            }
        }

        private void PlayStartNotification()
        {
            if (settings.StartNotificationVolume > 0)
            {
                PlaySound("src/start.mp3", "src/start.mp3", settings.StartNotificationVolume);
            }
        }

        private void PlayEndNotification()
        {
            if (settings.EndNotificationVolume > 0)
            {
                PlaySound("src/end.mp3", "src/end.mp3", settings.EndNotificationVolume);
            }
        }

        private void PlaySound(string customPath, string defaultPath, int volume)
        {
            try
            {
                string soundPath = File.Exists(customPath) ? customPath : defaultPath;

                if (File.Exists(soundPath))
                {
                    mediaPlayer.Open(new Uri(Path.GetFullPath(soundPath)));
                    mediaPlayer.Volume = (double)volume / 100.0 * (double)settings.MasterVolume / 100.0;
                    mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        private void PlayStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
                PlayStopButton.Content = "▶";
                StatusText.Text = "일시정지";
            }
            else
            {
                if (currentSeconds > 0)
                {
                    timer.Start();
                    isRunning = true;
                    PlayStopButton.Content = "⏸";
                    StatusText.Text = "실행 중";

                    if (currentSeconds == totalSeconds)
                    {
                        PlayStartNotification();
                        lastMinute = currentSeconds / 60;
                    }
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void ResetTimer()
        {
            timer.Stop();
            isRunning = false;
            currentSeconds = totalSeconds;
            lastMinute = -1;
            PlayStopButton.Content = "▶";
            StatusText.Text = "준비";
            UpdateDisplay();
        }


        private void MasterVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeLabel != null)
            {
                VolumeLabel.Text = $"{(int)e.NewValue}%";
                settings.MasterVolume = (int)e.NewValue;
                settings.Save();
            }
        }

        private void TimerProgressBar_Click(object sender, MouseButtonEventArgs e)
        {
            var progressBar = sender as System.Windows.Controls.ProgressBar;
            var position = e.GetPosition(progressBar);
            var clickedValue = position.X / progressBar.ActualWidth;

            currentSeconds = (int)(totalSeconds * (1 - clickedValue));
            UpdateDisplay();
        }

        private void TimerProgressBar_MouseMove(object sender, MouseEventArgs e)
        {
            var progressBar = sender as System.Windows.Controls.ProgressBar;
            var position = e.GetPosition(progressBar);
            var hoverValue = position.X / progressBar.ActualWidth;

            if (hoverValue >= 0 && hoverValue <= 1 && totalSeconds > 0)
            {
                int hoverSeconds = (int)(totalSeconds * (1 - hoverValue));
                int hours = hoverSeconds / 3600;
                int minutes = (hoverSeconds % 3600) / 60;
                int seconds = hoverSeconds % 60;

                string timeText;
                if (hours > 0)
                    timeText = $"{hours}:{minutes:D2}:{seconds:D2}";
                else
                    timeText = $"{minutes:D2}:{seconds:D2}";

                progressBar.ToolTip = timeText;
            }
        }

        private void TimerProgressBar_MouseLeave(object sender, MouseEventArgs e)
        {
            var progressBar = sender as System.Windows.Controls.ProgressBar;
            progressBar.ToolTip = null;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(settings);
            if (settingsWindow.ShowDialog() == true)
            {
                settings = settingsWindow.UpdatedSettings;
                LoadSettings();
                ResetTimer();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            // 창 위치가 변경될 때마다 저장
            if (this.IsLoaded)
            {
                SaveWindowPosition();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveWindowPosition();
            timer?.Stop();
            mediaPlayer?.Close();
            base.OnClosed(e);
        }
    }
}