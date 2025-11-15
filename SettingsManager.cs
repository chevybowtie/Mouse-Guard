using System.Text.Json;

namespace MouseGuard;

/// <summary>
/// Manages application settings storage in LocalAppData with error logging.
/// </summary>
public static class SettingsManager
{
    private const string AppFolderName = "Mouse-Guard";
    private const string SettingsFileName = "settings.json";
    private const string ErrorLogFileName = "error.log";

    // Centralized settings file path in LocalAppData
    private static readonly string LocalAppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    public static string SettingsFilePath { get; } = Path.Combine(LocalAppDataFolder, SettingsFileName);
    
    private static readonly string ErrorLogPath = Path.Combine(LocalAppDataFolder, ErrorLogFileName);
    
    // Legacy settings path in application directory
    private static readonly string LegacySettingsPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        SettingsFileName);

    /// <summary>
    /// Loads settings from disk or returns defaults.
    /// Automatically migrates from legacy location on first run.
    /// </summary>
    public static T LoadSettings<T>() where T : new()
    {
        try
        {
            // Check if migration is needed
            if (!File.Exists(SettingsFilePath) && File.Exists(LegacySettingsPath))
            {
                MigrateLegacySettings();
            }

            if (!File.Exists(SettingsFilePath))
                return new T();

            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch (Exception ex)
        {
            LogError($"Failed to load settings: {ex.Message}");
            return new T();
        }
    }

    /// <summary>
    /// Saves settings to disk in LocalAppData.
    /// </summary>
    public static void SaveSettings<T>(T settings)
    {
        try
        {
            // Ensure directory exists before writing
            EnsureDirectoryExists();

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            LogError($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensures the LocalAppData directory exists.
    /// </summary>
    private static void EnsureDirectoryExists()
    {
        if (!Directory.Exists(LocalAppDataFolder))
        {
            Directory.CreateDirectory(LocalAppDataFolder);
        }
    }

    /// <summary>
    /// Migrates settings from legacy application directory to LocalAppData.
    /// </summary>
    private static void MigrateLegacySettings()
    {
        try
        {
            EnsureDirectoryExists();
            
            // Copy the legacy settings file to new location
            File.Copy(LegacySettingsPath, SettingsFilePath, overwrite: false);
            
            LogError($"Settings migrated from {LegacySettingsPath} to {SettingsFilePath}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to migrate settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs errors to error.log in LocalAppData.
    /// </summary>
    private static void LogError(string message)
    {
        try
        {
            EnsureDirectoryExists();
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}{Environment.NewLine}";
            
            File.AppendAllText(ErrorLogPath, logMessage);
        }
        catch
        {
            // If we can't log, don't crash the app
        }
    }
}
