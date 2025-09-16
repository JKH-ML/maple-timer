using System;
using System.IO;
using System.Text.Json;

namespace 메이플_타이머
{
    public class Settings
    {
        public int DefaultTimerMinutes { get; set; } = 30;
        public int BasicNotificationVolume { get; set; } = 50;
        public int SpecialNotificationVolume { get; set; } = 50;
        public int StartNotificationVolume { get; set; } = 50;
        public int EndNotificationVolume { get; set; } = 50;
        public int MasterVolume { get; set; } = 50;
        public double WindowLeft { get; set; } = double.NaN;
        public double WindowTop { get; set; } = double.NaN;

        private static readonly string SettingsPath = "settings.json";

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                // Silent fail
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings: {ex.Message}");
            }
            return new Settings();
        }
    }
}