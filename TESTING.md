# Manual Testing Instructions for Settings Migration

## Overview
This document provides step-by-step instructions for manually verifying the settings migration from the application directory to LocalAppData.

## Prerequisites
- Windows operating system
- Mouse Guard application built and ready to run
- Access to file explorer to view settings files

## Test Scenarios

### Test 1: Fresh Installation (No Existing Settings)
**Purpose**: Verify that settings are created in LocalAppData on first run.

1. Ensure no settings file exists in either location:
   - Application directory: `<app-folder>\settings.json`
   - LocalAppData: `%LOCALAPPDATA%\Mouse-Guard\settings.json`
2. Run Mouse Guard
3. Select a screen to block
4. Exit the application
5. Verify:
   - Settings file exists at: `%LOCALAPPDATA%\Mouse-Guard\settings.json`
   - Settings file does NOT exist in application directory

### Test 2: Migration from Legacy Location
**Purpose**: Verify automatic migration from old location to new location.

1. Create a legacy settings file:
   - Navigate to the Mouse Guard application directory
   - Create `settings.json` with content:
     ```json
     {
       "BlockedScreenIndex": 1,
       "Hotkey": "Control,Alt,B"
     }
     ```
2. Ensure NO settings exist at: `%LOCALAPPDATA%\Mouse-Guard\settings.json`
3. Run Mouse Guard
4. Verify:
   - Settings file exists at: `%LOCALAPPDATA%\Mouse-Guard\settings.json`
   - Settings were correctly migrated (screen 1 is blocked)
   - Hotkey still works as configured
   - Check error.log at: `%LOCALAPPDATA%\Mouse-Guard\error.log` for migration message

### Test 3: Error Handling - Read-Only Directory
**Purpose**: Verify that application doesn't crash when unable to write settings.

1. Create the LocalAppData folder: `%LOCALAPPDATA%\Mouse-Guard\`
2. Set the folder to read-only
3. Run Mouse Guard
4. Try to change settings
5. Verify:
   - Application continues running without crashing
   - Error is logged to: `%LOCALAPPDATA%\Mouse-Guard\error.log` (if writable)
   - User can still interact with the application

### Test 4: Settings Persistence
**Purpose**: Verify settings persist across application restarts.

1. Run Mouse Guard
2. Configure:
   - Block a specific screen
   - Change hotkey to something custom (e.g., Ctrl+Shift+M)
3. Exit the application
4. Restart Mouse Guard
5. Verify:
   - Previously selected screen is still blocked
   - Custom hotkey still works
   - Settings file at `%LOCALAPPDATA%\Mouse-Guard\settings.json` contains correct values

### Test 5: Directory Creation
**Purpose**: Verify directory is created automatically if missing.

1. Delete the directory: `%LOCALAPPDATA%\Mouse-Guard\`
2. Run Mouse Guard
3. Change any setting (e.g., block a screen)
4. Verify:
   - Directory `%LOCALAPPDATA%\Mouse-Guard\` is created
   - Settings file is created inside it
   - No errors occur

## Expected File Locations

### Settings File
- **New Location**: `C:\Users\<username>\AppData\Local\Mouse-Guard\settings.json`
- **Legacy Location** (should NOT be used): `<application-directory>\settings.json`

### Error Log
- **Location**: `C:\Users\<username>\AppData\Local\Mouse-Guard\error.log`

## Validation Checklist

- [ ] Fresh install creates settings in LocalAppData
- [ ] Legacy settings are migrated on first run
- [ ] Settings persist across application restarts
- [ ] Directory is created if it doesn't exist
- [ ] Error handling prevents crashes
- [ ] Error log is created when errors occur
- [ ] Migration is logged in error.log
- [ ] Application behaves normally after migration

## Notes
- You can view LocalAppData by typing `%LOCALAPPDATA%` in Windows Explorer address bar
- The error.log file will only contain entries if errors occurred or migration happened
- Legacy settings file can be safely deleted after migration completes

## Automated Testing
If running on Windows with proper build environment, run unit tests:
```bash
dotnet test MouseGuard.Tests/MouseGuard.Tests.csproj
```

This will verify:
- Settings path resolves to LocalAppData
- Directory creation occurs before save
- Settings can be saved and loaded (round-trip)
- Migration from legacy location works correctly
- Default settings are returned when file doesn't exist
