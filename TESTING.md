# Settings Migration Testing Guide

This document describes how to test the settings migration from the application directory to LocalAppData.

## Testing the Implementation

### Prerequisites
- Build and run the Mouse Guard application on Windows

### Test 1: Verify Settings Location
**Objective**: Confirm settings are stored in LocalAppData

**Steps**:
1. Run Mouse Guard
2. Change any setting (e.g., select a screen to block or change hotkey)
3. Open File Explorer and navigate to: `%LOCALAPPDATA%\Mouse-Guard\`
4. Verify that `settings.json` exists in this directory

**Expected Result**: Settings file should be in `C:\Users\[YourUser]\AppData\Local\Mouse-Guard\settings.json`

---

### Test 2: Settings Directory Auto-Creation
**Objective**: Verify the directory is created automatically

**Steps**:
1. Close Mouse Guard if running
2. Delete the entire `%LOCALAPPDATA%\Mouse-Guard\` directory
3. Run Mouse Guard
4. Change a setting
5. Check if `%LOCALAPPDATA%\Mouse-Guard\` directory was created

**Expected Result**: Directory is created automatically when settings are saved

---

### Test 3: Migration from Application Directory
**Objective**: Verify automatic migration of existing settings

**Steps**:
1. Close Mouse Guard if running
2. Create a test `settings.json` in the application directory (where MouseGuard.exe is located) with content:
   ```json
   {
     "BlockedScreenIndex": 0,
     "Hotkey": "Control,Alt,B"
   }
   ```
3. Delete `%LOCALAPPDATA%\Mouse-Guard\settings.json` if it exists
4. Run Mouse Guard
5. Check that settings were migrated:
   - Verify `%LOCALAPPDATA%\Mouse-Guard\settings.json` now exists
   - Verify the blocked screen setting was preserved
   - Check `%LOCALAPPDATA%\Mouse-Guard\error.log` for migration message

**Expected Result**: 
- Settings file is copied from app directory to LocalAppData
- Settings are preserved (screen 0 is blocked)
- Migration is logged

---

### Test 4: Error Logging
**Objective**: Verify errors are logged properly

**Steps**:
1. Check `%LOCALAPPDATA%\Mouse-Guard\error.log` after migration
2. Look for log entries with timestamps and messages

**Expected Result**: Log file contains timestamped entries for any errors or the migration event

---

### Test 5: No File Locking Issues
**Objective**: Verify proper stream disposal

**Steps**:
1. Run Mouse Guard
2. While it's running, try to open `%LOCALAPPDATA%\Mouse-Guard\settings.json` in Notepad
3. Make a manual edit and save
4. Change a setting in Mouse Guard

**Expected Result**: 
- File can be opened and edited while app is running (no file locking)
- No errors occur

---

### Test 6: Automated Tests
**Objective**: Run the built-in test suite

The `SettingsManagerTests.cs` file contains automated tests that can be run to verify:
- Settings path resolution to LocalAppData
- Directory creation after save
- Settings save and load functionality

To run these tests, you would need to add a test runner or manually invoke them from a console app.

---

## Verification Checklist

After testing, verify:
- ✅ Settings file path resolves to `%LOCALAPPDATA%\Mouse-Guard\settings.json`
- ✅ Directory is created automatically before write operations
- ✅ Settings from app directory are migrated to LocalAppData
- ✅ Migration only happens once (when new location doesn't exist)
- ✅ Errors are logged to `error.log` in LocalAppData
- ✅ No file locking issues (streams are properly disposed)
- ✅ App continues to work even if settings cannot be read/written
- ✅ User settings are preserved after migration

## Common Issues

**Issue**: Settings not migrating
- **Solution**: Ensure the old `settings.json` exists in the app directory and the new location doesn't exist yet

**Issue**: Permission denied errors
- **Solution**: Check that the user has write access to `%LOCALAPPDATA%`

**Issue**: Settings reset to defaults
- **Solution**: Check `error.log` for error messages indicating what went wrong
