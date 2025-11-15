using System.Text.Json;

namespace MouseGuard.Tests
{
    /// <summary>
    /// Simple test class for verifying SettingsManager functionality.
    /// Run this as a console app to verify the implementation.
    /// </summary>
    internal static class SettingsManagerTests
    {
        /// <summary>
        /// Test settings class for verification.
        /// </summary>
        private class TestSettings
        {
            public string? TestValue { get; set; }
            public int TestNumber { get; set; }
        }

        /// <summary>
        /// Runs all tests and reports results.
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("=== SettingsManager Tests ===\n");

            int passed = 0;
            int failed = 0;

            // Test 1: Settings file path resolves to LocalAppData
            Console.Write("Test 1: Settings file path resolves to LocalAppData... ");
            if (TestSettingsPathResolution())
            {
                Console.WriteLine("PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("FAILED");
                failed++;
            }

            // Test 2: Directory creation
            Console.Write("Test 2: Directory exists after save operation... ");
            if (TestDirectoryCreation())
            {
                Console.WriteLine("PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("FAILED");
                failed++;
            }

            // Test 3: Settings save and load
            Console.Write("Test 3: Settings can be saved and loaded... ");
            if (TestSaveAndLoad())
            {
                Console.WriteLine("PASSED");
                passed++;
            }
            else
            {
                Console.WriteLine("FAILED");
                failed++;
            }

            // Test 4: Migration simulation (manual verification needed)
            Console.Write("Test 4: Migration logic (manual verification)... ");
            TestMigrationInfo();
            Console.WriteLine("INFO (see output)");

            Console.WriteLine($"\n=== Results: {passed} passed, {failed} failed ===\n");
        }

        /// <summary>
        /// Test 1: Verify settings file path resolves to LocalAppData.
        /// </summary>
        private static bool TestSettingsPathResolution()
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string expectedPath = Path.Combine(localAppData, "Mouse-Guard", "settings.json");
                
                bool result = SettingsManager.SettingsFilePath == expectedPath;
                if (!result)
                {
                    Console.WriteLine($"\n  Expected: {expectedPath}");
                    Console.WriteLine($"  Actual: {SettingsManager.SettingsFilePath}");
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 2: Verify directory is created after save operation.
        /// </summary>
        private static bool TestDirectoryCreation()
        {
            try
            {
                // Save a test setting
                var testSettings = new TestSettings { TestValue = "Test", TestNumber = 42 };
                SettingsManager.SaveSettings(testSettings);

                // Check if directory exists
                string settingsDir = Path.GetDirectoryName(SettingsManager.SettingsFilePath) ?? "";
                bool result = Directory.Exists(settingsDir);
                
                if (!result)
                {
                    Console.WriteLine($"\n  Directory not found: {settingsDir}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 3: Verify settings can be saved and loaded correctly.
        /// </summary>
        private static bool TestSaveAndLoad()
        {
            try
            {
                // Save test settings
                var originalSettings = new TestSettings
                {
                    TestValue = "TestValue123",
                    TestNumber = 999
                };
                SettingsManager.SaveSettings(originalSettings);

                // Load settings
                var loadedSettings = SettingsManager.LoadSettings<TestSettings>();

                // Verify values match
                bool result = loadedSettings.TestValue == originalSettings.TestValue &&
                              loadedSettings.TestNumber == originalSettings.TestNumber;

                if (!result)
                {
                    Console.WriteLine($"\n  Original: TestValue={originalSettings.TestValue}, TestNumber={originalSettings.TestNumber}");
                    Console.WriteLine($"  Loaded: TestValue={loadedSettings.TestValue}, TestNumber={loadedSettings.TestNumber}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n  Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 4: Display information about migration behavior.
        /// </summary>
        private static void TestMigrationInfo()
        {
            Console.WriteLine("\n  Migration Behavior:");
            Console.WriteLine($"  - New settings location: {SettingsManager.SettingsFilePath}");
            Console.WriteLine($"  - Legacy location: {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json")}");
            Console.WriteLine("  - To test migration: Place a settings.json in the app directory and delete the one in LocalAppData,");
            Console.WriteLine("    then run the app. The file should be copied to the new location.");
        }
    }

    /// <summary>
    /// Manual test notes for verification:
    /// 
    /// 1. Settings Path Resolution:
    ///    - Verify settings are stored in %LOCALAPPDATA%\Mouse-Guard\settings.json
    ///    - Run: echo %LOCALAPPDATA%\Mouse-Guard\settings.json
    /// 
    /// 2. Directory Creation:
    ///    - Delete %LOCALAPPDATA%\Mouse-Guard directory
    ///    - Run the app and change a setting
    ///    - Verify the directory is created automatically
    /// 
    /// 3. Migration from App Directory:
    ///    - Create a settings.json file in the application directory with test content
    ///    - Delete %LOCALAPPDATA%\Mouse-Guard\settings.json if it exists
    ///    - Run the app
    ///    - Verify the settings file is copied to %LOCALAPPDATA%\Mouse-Guard\
    ///    - Verify settings are preserved (blocked screen index, hotkey)
    /// 
    /// 4. Error Logging:
    ///    - Simulate an error condition (e.g., read-only LocalAppData folder)
    ///    - Verify errors are logged to %LOCALAPPDATA%\Mouse-Guard\error.log
    /// 
    /// 5. Stream Disposal:
    ///    - Check that settings files can be opened/edited while app is running
    ///    - Verify no file locking issues
    /// </summary>
    internal static class ManualTestNotes { }
}
