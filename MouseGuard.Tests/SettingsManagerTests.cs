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

    [Fact]
    public void SaveSettings_IsAtomic_NoPartialWrites()
    {
        // Arrange
        var testSettings = new TestSettings { Value = "atomic-test-value" };

        // Act
        SettingsManager.SaveSettings(testSettings);

        // Assert - File should exist and be complete (not partial)
        Assert.True(File.Exists(SettingsManager.SettingsFilePath));
        
        // Verify we can read it back successfully (would fail if partial write)
        var loadedSettings = SettingsManager.LoadSettings<TestSettings>();
        Assert.NotNull(loadedSettings);
        Assert.Equal(testSettings.Value, loadedSettings.Value);
        
        // Verify no temp file is left behind
        Assert.False(File.Exists(SettingsManager.SettingsFilePath + ".tmp"));
    }

    [Fact]
    public void MigrateOldSettings_ValidatesJson_DeletesInvalidFile()
    {
        // Arrange - Clean up existing files
        if (File.Exists(SettingsManager.SettingsFilePath))
        {
            File.Delete(SettingsManager.SettingsFilePath);
        }

        // Create an invalid JSON file in old location
        var oldPath = SettingsManager.OldSettingsPath;
        File.WriteAllText(oldPath, "{ invalid json ]");

        try
        {
            // Act
            SettingsManager.MigrateOldSettings();

            // Assert - Migration should detect invalid JSON and delete it
            Assert.False(File.Exists(SettingsManager.SettingsFilePath), 
                "Invalid migrated file should be deleted");
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

    [Fact]
    public void MigrateOldSettings_ValidatesJson_KeepsValidFile()
    {
        // Arrange - Clean up existing files
        if (File.Exists(SettingsManager.SettingsFilePath))
        {
            File.Delete(SettingsManager.SettingsFilePath);
        }

        // Create a valid JSON file in old location
        var oldPath = SettingsManager.OldSettingsPath;
        var testSettings = new TestSettings { Value = "valid-migrated" };
        var validJson = System.Text.Json.JsonSerializer.Serialize(testSettings);
        File.WriteAllText(oldPath, validJson);

        try
        {
            // Act
            SettingsManager.MigrateOldSettings();

            // Assert - Valid file should be kept
            Assert.True(File.Exists(SettingsManager.SettingsFilePath), 
                "Valid migrated file should exist");
            
            var loadedSettings = SettingsManager.LoadSettings<TestSettings>();
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

    [Fact]
    public void ConcurrentAccess_MultipleThreads_NoCorruption()
    {
        // Arrange
        var tasks = new List<Task>();
        var iterations = 10;

        // Act - Multiple threads writing and reading concurrently
        for (int i = 0; i < 5; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    var settings = new TestSettings { Value = $"thread-{threadId}-iteration-{j}" };
                    SettingsManager.SaveSettings(settings);
                    var loaded = SettingsManager.LoadSettings<TestSettings>();
                    Assert.NotNull(loaded);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert - Final read should succeed (no corruption)
        var finalSettings = SettingsManager.LoadSettings<TestSettings>();
        Assert.NotNull(finalSettings);
        Assert.NotNull(finalSettings.Value);
    }

    [Fact]
    public void EnsureLocalAppDataDirectoryExists_WithErrorHandling_DoesNotThrow()
    {
        // Act & Assert - Should not throw even if there are issues
        // (In normal circumstances it will create the directory)
        var exception = Record.Exception(() => SettingsManager.EnsureLocalAppDataDirectoryExists());
        Assert.Null(exception);
    }

    // Helper class for testing
    private class TestSettings
    {
        public string? Value { get; set; }
    }
}
