using System.Text.Json;

namespace MouseGuard
{
    /// <summary>
    /// Manages application settings storage in LocalAppData.
    /// </summary>
    internal static class SettingsManager
    {
        private const string AppFolderName = "Mouse-Guard";
        private const string SettingsFileName = "settings.json";
        private const string ErrorLogFileName = "error.log";

        // Path to settings directory in LocalAppData
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppFolderName);

        // Full path to settings file
        public static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);

        // Full path to error log file
        private static readonly string ErrorLogPath = Path.Combine(SettingsDirectory, ErrorLogFileName);

        // Legacy settings path (application directory)
        private static readonly string LegacySettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            SettingsFileName);

        /// <summary>
        /// Ensures the settings directory exists.
        /// </summary>
        private static void EnsureSettingsDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create settings directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Migrates settings from legacy location (app directory) to LocalAppData if needed.
        /// </summary>
        private static void MigrateLegacySettings()
        {
            try
            {
                // Only migrate if new location doesn't exist but old one does
                if (!File.Exists(SettingsFilePath) && File.Exists(LegacySettingsPath))
                {
                    EnsureSettingsDirectoryExists();
                    File.Copy(LegacySettingsPath, SettingsFilePath, overwrite: false);
                    LogError($"Migrated settings from {LegacySettingsPath} to {SettingsFilePath}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to migrate legacy settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads settings from disk or returns defaults.
        /// </summary>
        public static T LoadSettings<T>() where T : new()
        {
            // Attempt migration on first load
            MigrateLegacySettings();

            if (!File.Exists(SettingsFilePath))
            {
                return new T();
            }

            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<T>(json);
                return settings ?? new T();
            }
            catch (Exception ex)
            {
                LogError($"Failed to load settings: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// Saves settings to disk.
        /// </summary>
        public static void SaveSettings<T>(T settings)
        {
            try
            {
                EnsureSettingsDirectoryExists();
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                LogError($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs an error message to the error log file in LocalAppData.
        /// </summary>
        private static void LogError(string message)
        {
            try
            {
                EnsureSettingsDirectoryExists();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(ErrorLogPath, logEntry);
            }
            catch
            {
                // If we can't log, silently fail to avoid cascading errors
            }
        }
    }
}
