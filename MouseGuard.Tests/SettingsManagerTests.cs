using MouseGuard;

namespace MouseGuard.Tests;

public class SettingsManagerTests
{
    private class TestSettings
    {
        public string? TestValue { get; set; }
        public int TestNumber { get; set; }
    }

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
    public void SettingsFilePath_ContainsMouseGuardFolder()
    {
        // Act
        var path = SettingsManager.SettingsFilePath;

        // Assert
        Assert.Contains("Mouse-Guard", path);
        Assert.Contains("settings.json", path);
    }

    [Fact]
    public void SaveSettings_CreatesDirectory()
    {
        // Arrange
        var settings = new TestSettings { TestValue = "test", TestNumber = 42 };
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var expectedDirectory = Path.Combine(localAppData, "Mouse-Guard");

        // Act
        SettingsManager.SaveSettings(settings);

        // Assert
        Assert.True(Directory.Exists(expectedDirectory), "Directory should be created in LocalAppData");
    }

    [Fact]
    public void SaveAndLoadSettings_RoundTrip()
    {
        // Arrange
        var originalSettings = new TestSettings { TestValue = "hello world", TestNumber = 123 };

        // Act
        SettingsManager.SaveSettings(originalSettings);
        var loadedSettings = SettingsManager.LoadSettings<TestSettings>();

        // Assert
        Assert.Equal(originalSettings.TestValue, loadedSettings.TestValue);
        Assert.Equal(originalSettings.TestNumber, loadedSettings.TestNumber);
    }

    [Fact]
    public void LoadSettings_ReturnsDefaultWhenFileDoesNotExist()
    {
        // Arrange - ensure file doesn't exist by deleting it if present
        if (File.Exists(SettingsManager.SettingsFilePath))
        {
            File.Delete(SettingsManager.SettingsFilePath);
        }

        // Act
        var settings = SettingsManager.LoadSettings<TestSettings>();

        // Assert
        Assert.NotNull(settings);
        Assert.Null(settings.TestValue);
        Assert.Equal(0, settings.TestNumber);
    }

    [Fact]
    public void MigrationTest_CopiesFromLegacyLocation()
    {
        // Arrange
        var legacyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        var newPath = SettingsManager.SettingsFilePath;
        
        // Clean up any existing files
        if (File.Exists(newPath))
        {
            File.Delete(newPath);
        }

        // Create a legacy settings file
        var legacySettings = new TestSettings { TestValue = "legacy", TestNumber = 999 };
        var json = System.Text.Json.JsonSerializer.Serialize(legacySettings);
        File.WriteAllText(legacyPath, json);

        try
        {
            // Act - Load should trigger migration
            var loadedSettings = SettingsManager.LoadSettings<TestSettings>();

            // Assert
            Assert.True(File.Exists(newPath), "Settings file should exist in new location after migration");
            Assert.Equal("legacy", loadedSettings.TestValue);
            Assert.Equal(999, loadedSettings.TestNumber);
        }
        finally
        {
            // Cleanup
            if (File.Exists(legacyPath))
            {
                File.Delete(legacyPath);
            }
        }
    }
}
