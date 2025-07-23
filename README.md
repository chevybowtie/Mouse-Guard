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

- [ ] Update tray menu dynamically if monitors are added/removed
- [ ] Improve error reporting and logging
- [ ] Add accessibility features
- [x] Add localization/multi-language support

## License

MIT License