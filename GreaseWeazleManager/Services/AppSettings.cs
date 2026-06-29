using System;
using System.IO;
using System.Text.Json;

namespace GwCopyPro.Services
{
    /// <summary>UI language choices available in <see cref="AppSettings"/>.</summary>
    public enum AppLanguage { English, German }

    /// <summary>
    /// Singleton that persists user preferences to
    /// <c>%APPDATA%\GreaseWeazleManager\settings.json</c>.
    /// </summary>
    public class AppSettings
    {
        /// <summary>Full path to the <c>gw.exe</c> binary. Defaults to <c>"gw.exe"</c> (resolved via PATH).</summary>
        public string  GwExePath { get; set; } = "gw.exe";

        /// <summary>Active UI language. Defaults to <see cref="AppLanguage.English"/>.</summary>
        public AppLanguage Language { get; set; } = AppLanguage.English;

        private static AppSettings? _instance;

        /// <summary>Gets the process-wide singleton, loading it from disk on first access.</summary>
        public static AppSettings Instance => _instance ??= Load();

        private static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GreaseWeazleManager",
                "settings.json");

        /// <summary>
        /// Loads settings from disk, returning defaults if the file does not exist or cannot be parsed.
        /// </summary>
        /// <returns>A populated <see cref="AppSettings"/> instance.</returns>
        public static AppSettings Load()
        {
            try
            {
                string path = SettingsPath;
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var s = JsonSerializer.Deserialize<AppSettings>(json);
                    if (s != null) return s;
                }
            }
            catch { }
            return new AppSettings();
        }

        /// <summary>
        /// Persists the current settings to disk.
        /// Failures are silently swallowed because settings loss is non-critical.
        /// </summary>
        public void Save()
        {
            try
            {
                string path = SettingsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                string json = JsonSerializer.Serialize(this,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }
        }
    }
}
