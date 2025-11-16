using System;
using System.Text.Json;

namespace MouseGuard;

/// <summary>
/// Manages application settings storage in LocalApplicationData.
/// Provides centralized path management, migration, error handling, atomic writes, and concurrency protection.
/// </summary>
public static class SettingsManager
{
    private static readonly string LocalAppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Mouse-Guard");

    private static readonly object _lockObject = new object();

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
    /// Ensures the LocalApplicationData directory exists with defensive error handling.
    /// </summary>
    public static void EnsureLocalAppDataDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(LocalAppDataFolder))
            {
                Directory.CreateDirectory(LocalAppDataFolder);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - caller should handle gracefully
            LogError($"Failed to create directory {LocalAppDataFolder}", ex);
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
                logEntry += Environment.NewLine + $"{ex.GetType().Name}: {ex.Message}" + Environment.NewLine + ex.StackTrace;
            logEntry += Environment.NewLine;
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
    /// Includes validation to ensure migrated file is readable JSON.
    /// </summary>
    public static void MigrateOldSettings()
    {
        lock (_lockObject)
        {
            try
            {
                if (!File.Exists(SettingsFilePath) && File.Exists(OldSettingsPath))
                {
                    EnsureLocalAppDataDirectoryExists();
                    
                    // Copy file
                    File.Copy(OldSettingsPath, SettingsFilePath, overwrite: false);
                    
                    // Validate the migrated file is readable JSON
                    try
                    {
                        var testJson = File.ReadAllText(SettingsFilePath);
                        JsonDocument.Parse(testJson).Dispose(); // Validate JSON structure
                        LogError("Settings migrated successfully from application directory to LocalAppData");
                    }
                    catch (JsonException)
                    {
                        // Migration file is invalid JSON, delete it and log error
                        File.Delete(SettingsFilePath);
                        LogError("Migrated settings file was invalid JSON, deleted it. Using defaults instead.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to migrate old settings", ex);
            }
        }
    }

    /// <summary>
    /// Loads settings from disk with concurrency protection.
    /// </summary>
    public static T? LoadSettings<T>() where T : new()
    {
        lock (_lockObject)
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
    }

    /// <summary>
    /// Saves settings to disk atomically with concurrency protection.
    /// Uses atomic write pattern: write to temp file, then rename to final location.
    /// </summary>
    public static void SaveSettings<T>(T settings)
    {
        lock (_lockObject)
        {
            try
            {
                EnsureLocalAppDataDirectoryExists();
                
                // Atomic write: write to temporary file first
                var tempPath = SettingsFilePath + ".tmp";
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                
                // Write to temp file
                File.WriteAllText(tempPath, json);
                
                // Verify the temp file is valid JSON before replacing the real file
                try
                {
                    JsonDocument.Parse(json).Dispose();
                }
                catch (JsonException)
                {
                    // Delete invalid temp file
                    File.Delete(tempPath);
                    LogError("Failed to save settings: generated JSON was invalid");
                    return;
                }
                
                // Atomically replace the old file with the new one
                File.Move(tempPath, SettingsFilePath, overwrite: true);
            }
            catch (Exception ex)
            {
                LogError("Failed to save settings", ex);
            }
        }
    }
}
