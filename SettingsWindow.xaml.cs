using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace 메이플_타이머
{
    public partial class SettingsWindow : Window
    {
        private Settings originalSettings;
        public Settings UpdatedSettings { get; private set; }

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            originalSettings = settings;
            UpdatedSettings = new Settings
            {
                DefaultTimerMinutes = settings.DefaultTimerMinutes,
                BasicNotificationVolume = settings.BasicNotificationVolume,
                SpecialNotificationVolume = settings.SpecialNotificationVolume,
                StartNotificationVolume = settings.StartNotificationVolume,
                EndNotificationVolume = settings.EndNotificationVolume,
                MasterVolume = settings.MasterVolume
            };

            LoadSettings();
        }

        private void LoadSettings()
        {
            DefaultTimerSlider.Value = UpdatedSettings.DefaultTimerMinutes;
            BasicVolumeSlider.Value = UpdatedSettings.BasicNotificationVolume;
            SpecialVolumeSlider.Value = UpdatedSettings.SpecialNotificationVolume;
            StartVolumeSlider.Value = UpdatedSettings.StartNotificationVolume;
            EndVolumeSlider.Value = UpdatedSettings.EndNotificationVolume;

            UpdateLabels();
            UpdateSoundLabels();
        }

        private void UpdateLabels()
        {
            DefaultTimerLabel.Text = $"{(int)DefaultTimerSlider.Value}분";
            BasicVolumeLabel.Text = $"{(int)BasicVolumeSlider.Value}%";
            SpecialVolumeLabel.Text = $"{(int)SpecialVolumeSlider.Value}%";
            StartVolumeLabel.Text = $"{(int)StartVolumeSlider.Value}%";
            EndVolumeLabel.Text = $"{(int)EndVolumeSlider.Value}%";
        }

        private void UpdateSoundLabels()
        {
            BasicSoundLabel.Text = File.Exists("src/1-min-custom.mp3") ? "커스텀" : "기본값";
            SpecialSoundLabel.Text = File.Exists("src/30-min-custom.mp3") ? "커스텀" : "기본값";
        }

        private void DefaultTimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DefaultTimerLabel != null)
            {
                UpdatedSettings.DefaultTimerMinutes = (int)e.NewValue;
                DefaultTimerLabel.Text = $"{(int)e.NewValue}분";
            }
        }

        private void BasicVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BasicVolumeLabel != null)
            {
                UpdatedSettings.BasicNotificationVolume = (int)e.NewValue;
                BasicVolumeLabel.Text = $"{(int)e.NewValue}%";
            }
        }

        private void SpecialVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SpecialVolumeLabel != null)
            {
                UpdatedSettings.SpecialNotificationVolume = (int)e.NewValue;
                SpecialVolumeLabel.Text = $"{(int)e.NewValue}%";
            }
        }

        private void StartVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (StartVolumeLabel != null)
            {
                UpdatedSettings.StartNotificationVolume = (int)e.NewValue;
                StartVolumeLabel.Text = $"{(int)e.NewValue}%";
            }
        }

        private void EndVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (EndVolumeLabel != null)
            {
                UpdatedSettings.EndNotificationVolume = (int)e.NewValue;
                EndVolumeLabel.Text = $"{(int)e.NewValue}%";
            }
        }

        private void BasicSoundButton_Click(object sender, RoutedEventArgs e)
        {
            SelectSoundFile("src/1-min-custom.mp3");
            UpdateSoundLabels();
        }

        private void SpecialSoundButton_Click(object sender, RoutedEventArgs e)
        {
            SelectSoundFile("src/30-min-custom.mp3");
            UpdateSoundLabels();
        }

        private void SelectSoundFile(string targetPath)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Audio files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*",
                Title = "사운드 파일 선택"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Directory.CreateDirectory("src");
                    File.Copy(openFileDialog.FileName, targetPath, true);
                    MessageBox.Show("사운드 파일이 성공적으로 설정되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 복사 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists("src/1-min-custom.mp3"))
                    File.Delete("src/1-min-custom.mp3");
                if (File.Exists("src/30-min-custom.mp3"))
                    File.Delete("src/30-min-custom.mp3");

                UpdatedSettings = new Settings();
                LoadSettings();
                MessageBox.Show("설정이 기본값으로 초기화되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"초기화 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatedSettings.Save();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}