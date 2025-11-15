using System.Text.Json;

namespace MouseGuard;

/// <summary>
/// Manages application settings storage in LocalApplicationData.
/// Provides centralized path management, migration, and error handling.
/// </summary>
public static class SettingsManager
{
    private static readonly string LocalAppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Mouse-Guard");

    /// <summary>
    /// Gets the path to the settings file in LocalApplicationData.
    /// </summary>
    public static string SettingsFilePath => Path.Combine(LocalAppDataFolder, "settings.json");

    /// <summary>
    /// Gets the path to the error log file in LocalApplicationData.
    /// </summary>
    public static string ErrorLogPath => Path.Combine(LocalAppDataFolder, "error.log");

    /// <summary>
    /// Gets the path to the old settings file in the application directory.
    /// </summary>
    public static string OldSettingsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    /// <summary>
    /// Ensures the LocalApplicationData directory exists.
    /// </summary>
    public static void EnsureLocalAppDataDirectoryExists()
    {
        if (!Directory.Exists(LocalAppDataFolder))
        {
            Directory.CreateDirectory(LocalAppDataFolder);
        }
    }

    /// <summary>
    /// Logs an error message to the error log file.
    /// </summary>
    public static void LogError(string message, Exception? ex = null)
    {
        try
        {
            EnsureLocalAppDataDirectoryExists();
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            if (ex != null)
                logEntry += $"\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}";
            logEntry += "\n";
            File.AppendAllText(ErrorLogPath, logEntry);
        }
        catch
        {
            // If logging fails, silently continue - we don't want logging to crash the app
        }
    }

    /// <summary>
    /// Migrates settings from the old location (app directory) to the new location (LocalAppData).
    /// Called on startup if the new settings file doesn't exist but the old one does.
    /// </summary>
    public static void MigrateOldSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath) && File.Exists(OldSettingsPath))
            {
                EnsureLocalAppDataDirectoryExists();
                File.Copy(OldSettingsPath, SettingsFilePath, overwrite: false);
                LogError("Settings migrated from application directory to LocalAppData");
            }
        }
        catch (Exception ex)
        {
            LogError("Failed to migrate old settings", ex);
        }
    }

    /// <summary>
    /// Loads settings from disk.
    /// </summary>
    public static T? LoadSettings<T>() where T : new()
    {
        if (!File.Exists(SettingsFilePath))
            return new T();

        try
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch (Exception ex)
        {
            LogError("Failed to load settings", ex);
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
            EnsureLocalAppDataDirectoryExists();
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            LogError("Failed to save settings", ex);
        }
    }
}
