# MouseGuard

MouseGuard is a Windows utility that blocks the mouse cursor from entering a selected screen. It runs in the system tray and allows you to choose which monitor to block via a context menu.

## Purpose & Use Cases

MouseGuard is especially useful in scenarios where you want to prevent the mouse cursor from accidentally appearing on a public or presentation screen. For example:

- **Presentations & Lectures:** Prevent the mouse from showing up on the auditorium or projector screen while presenting, ensuring a distraction-free experience for your audience.
- **Digital Signage:** Block the mouse from entering a display used for digital signage or information kiosks.
- **Kiosk/Exhibit Setups:** Keep the mouse confined to an operator screen and away from a customer-facing or public monitor.
- **Multi-monitor Workstations:** Avoid accidental mouse movement onto a sensitive or shared display.

## Features

- Block the mouse from entering a selected screen
- System tray icon with context menu for screen selection
- Remembers blocked screen between sessions
- Friendly monitor names (when available)
- Minimal, lightweight, and easy to use
- **Global hotkey** to enable/disable blocking (default: Ctrl+Alt+B)
- Hotkey is configurable via the "Advanced Settings..." menu
- **Shows a notification when the mouse is blocked**

## Usage

1. Run `MouseGuard.exe`.
2. Right-click the tray icon to select which screen to block.
3. The mouse will be prevented from entering the blocked screen.
4. Use the global hotkey (default: Ctrl+Alt+B) to toggle blocking on/off.
5. To change the hotkey, right-click the tray icon and choose **Advanced Settings...**.
6. To exit, use the "Exit" option in the tray menu.

## Settings

Settings (blocked screen and hotkey) are saved to `settings.json` in the application directory.

## Build / Compile

You can build MouseGuard using Visual Studio or the .NET CLI.

### Using Visual Studio

1. Open the solution or project folder in Visual Studio.
2. Make sure you have the .NET Desktop Development workload installed.
3. Build the solution (Ctrl+Shift+B).
4. The output executable will be in the `bin\Release\net6.0-windows` or `bin\Release\net8.0-windows` folder, depending on your target framework.

### Using .NET CLI

1. Open a terminal or command prompt in the project directory.
2. Run:

   ```
   dotnet build -c Release
   ```

3. The executable will be in the `bin\Release\<target-framework>` directory.

**Requirements:**  
- .NET 6.0 (or newer) with Windows desktop support (`net6.0-windows` or `net8.0-windows`).
- Windows OS.

## TODO

### Implemented

- [x] Add localization/multi-language support
- [x] Move settings to LocalAppData and ensure directory exists. Settings now stored in `%LOCALAPPDATA%\\Mouse-Guard\\settings.json` with automatic migration from old location on first run.
- [x] Store monitor DeviceName (or EDID) instead of screen index so user selection survives monitor reordering
- [x] If a user only has one monitor connected/detected, show a warning and disable blocking functionality; dynamically detect monitor count changes and re-enable functionality when a second monitor is connected.
- [x] Add icon fallback and dispose file-loaded icon on exit; ensure resources (`icon`, `trayIcon`, `timers`, `notifications`) are properly disposed on exit.
- [x] Only allow one copy to run at a time
- [x] Dispose `trayIcon` and `monitorTimer` and close `silentNotification` on Exit

### Pending

- [ ] Improve error reporting and logging
- [ ] Add accessibility features
- [ ] Validate parsed hotkey has a key code before accepting
- [ ] Check `RegisterHotKey` result and reflect failure (disable hotkey or notify)
- [ ] Avoid swallowing exceptions — log to a simple file or at least debug output
- [ ] Replace broad `catch { }` with logging to a file in LocalAppData for later diagnostics
- [ ] Move heavy operations (ManagementObjectSearcher) off the UI thread or cache results

### Testing

- [ ] Add explicit unit tests for hotkey parsing and settings read/write, and add a small logging mechanism to capture runtime failures

## License

MIT License