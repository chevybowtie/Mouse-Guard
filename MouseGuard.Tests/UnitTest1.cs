namespace MouseGuard.Tests;

/// <summary>
/// Tests for the SettingsManager class to verify settings path resolution, 
/// directory creation, and migration behavior.
/// </summary>
public class SettingsManagerTests
{
    [Fact]
    public void SettingsFilePath_ResolvesToLocalAppData()
    {
        // Arrange
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedPath = Path.Combine(localAppData, "Mouse-Guard", "settings.json");

        // Act
        var actualPath = SettingsManager.SettingsFilePath;

        // Assert
        Assert.Equal(expectedPath, actualPath);
    }

    [Fact]
    public void ErrorLogPath_ResolvesToLocalAppData()
    {
        // Arrange
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedPath = Path.Combine(localAppData, "Mouse-Guard", "error.log");

        // Act
        var actualPath = SettingsManager.ErrorLogPath;

        // Assert
        Assert.Equal(expectedPath, actualPath);
    }

    [Fact]
    public void EnsureLocalAppDataDirectoryExists_CreatesDirectory()
    {
        // Arrange
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedDir = Path.Combine(localAppData, "Mouse-Guard");

        // Act
        SettingsManager.EnsureLocalAppDataDirectoryExists();

        // Assert
        Assert.True(Directory.Exists(expectedDir), "Directory should exist after calling EnsureLocalAppDataDirectoryExists");
    }

    [Fact]
    public void SaveSettings_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedDir = Path.Combine(localAppData, "Mouse-Guard");
        var testSettings = new TestSettings { Value = "test" };

        // Act
        SettingsManager.SaveSettings(testSettings);

        // Assert
        Assert.True(Directory.Exists(expectedDir), "Directory should exist after saving settings");
        Assert.True(File.Exists(SettingsManager.SettingsFilePath), "Settings file should exist after saving");
    }

    [Fact]
    public void LoadSettings_ReturnsDefaultWhenFileDoesNotExist()
    {
        // Arrange
        if (File.Exists(SettingsManager.SettingsFilePath))
        {
            File.Delete(SettingsManager.SettingsFilePath);
        }

        // Act
        var settings = SettingsManager.LoadSettings<TestSettings>();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(default(string), settings.Value);
    }

    [Fact]
    public void SaveAndLoadSettings_RoundTrip()
    {
        // Arrange
        var testSettings = new TestSettings { Value = "test-value-123" };

        // Act
        SettingsManager.SaveSettings(testSettings);
        var loadedSettings = SettingsManager.LoadSettings<TestSettings>();

        // Assert
        Assert.NotNull(loadedSettings);
        Assert.Equal(testSettings.Value, loadedSettings.Value);
    }

    [Fact]
    public void MigrateOldSettings_CopiesFileWhenOldExists()
    {
        // Arrange - Clean up any existing files
        if (File.Exists(SettingsManager.SettingsFilePath))
        {
            File.Delete(SettingsManager.SettingsFilePath);
        }

        // Create an old settings file
        var oldPath = SettingsManager.OldSettingsPath;
        var testSettings = new TestSettings { Value = "migrated-value" };
        var json = System.Text.Json.JsonSerializer.Serialize(testSettings);
        File.WriteAllText(oldPath, json);

        try
        {
            // Act
            SettingsManager.MigrateOldSettings();

            // Assert
            Assert.True(File.Exists(SettingsManager.SettingsFilePath), "New settings file should exist after migration");
            var loadedSettings = SettingsManager.LoadSettings<TestSettings>();
            Assert.NotNull(loadedSettings);
            Assert.Equal(testSettings.Value, loadedSettings.Value);
        }
        finally
        {
            // Cleanup
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }
    }

    // Helper class for testing
    private class TestSettings
    {
        public string? Value { get; set; }
    }
}
