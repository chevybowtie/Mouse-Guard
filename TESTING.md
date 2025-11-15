# Manual Testing Instructions for Settings Migration

Since this is a Windows-specific application, automated tests cannot run on all platforms. Follow these steps to manually verify the settings migration functionality:

## Test 1: New Installation (No Existing Settings)
1. Delete any existing settings files:
   - `%LOCALAPPDATA%\Mouse-Guard\settings.json`
   - Application directory `settings.json` (next to the executable)
2. Run Mouse Guard
3. Configure a blocked screen and/or change the hotkey
4. Close the application
5. Verify that `%LOCALAPPDATA%\Mouse-Guard\settings.json` was created
6. Verify the directory structure exists: `%LOCALAPPDATA%\Mouse-Guard\`
7. Restart Mouse Guard and verify your settings were preserved

**Expected Result:** Settings are saved to LocalAppData and persist across restarts.

## Test 2: Migration from Old Location
1. Delete `%LOCALAPPDATA%\Mouse-Guard\settings.json` if it exists
2. Create a settings file in the application directory (next to the .exe) with the following content:
   ```json
   {
     "BlockedScreenIndex": 1,
     "Hotkey": "Control,Alt,B"
   }
   ```
3. Run Mouse Guard
4. Verify that:
   - `%LOCALAPPDATA%\Mouse-Guard\settings.json` now exists
   - The settings from the old location were migrated (screen 2 is blocked)
   - Check `%LOCALAPPDATA%\Mouse-Guard\error.log` for a migration message

**Expected Result:** Old settings are copied to the new location and loaded correctly.

## Test 3: Error Handling
1. Create `%LOCALAPPDATA%\Mouse-Guard\settings.json` with invalid JSON:
   ```
   { invalid json
   ```
2. Run Mouse Guard
3. Verify that:
   - The application doesn't crash
   - Default settings are used
   - An error is logged to `%LOCALAPPDATA%\Mouse-Guard\error.log`

**Expected Result:** Application handles corrupt settings gracefully and logs the error.

## Test 4: Directory Creation
1. Delete the entire `%LOCALAPPDATA%\Mouse-Guard\` directory
2. Run Mouse Guard
3. Configure a blocked screen
4. Verify that the directory was automatically created

**Expected Result:** The LocalAppData directory is created automatically when needed.

## Unit Tests
The automated unit tests in `MouseGuard.Tests` verify:
- Settings file path resolves to `%LOCALAPPDATA%\Mouse-Guard\settings.json`
- Directory creation works correctly
- Settings can be saved and loaded
- Migration logic copies files when appropriate

To run the unit tests on Windows:
```bash
dotnet test
```

Note: Some tests may fail on non-Windows platforms due to Windows-specific paths and APIs.
