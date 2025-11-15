# GitHub Copilot Instructions for Mouse-Guard

## Project Overview

Mouse-Guard is a Windows utility application that blocks the mouse cursor from entering a selected screen. It's particularly useful for presentations, digital signage, kiosk setups, and multi-monitor workstations where you want to prevent accidental mouse movement onto specific displays.

## Tech Stack

- **Language**: C# with .NET 8.0
- **Framework**: Windows Forms (.NET 8.0-windows)
- **Target Platform**: Windows (x64)
- **Key Dependencies**:
  - System.Management (v9.0.7)
  - Windows Forms
  - Win32 APIs (user32.dll for cursor control)

## Project Structure

- `Program.cs` - Main application logic, mouse blocking, hotkey handling, and system tray functionality
- `MainForm.cs` / `MainForm.Designer.cs` - Windows Form components
- `Strings.cs` - Localized string resources
- `Resources.*.resx` - Resource files for internationalization (English, German, Spanish)
- `MouseGuard.csproj` - Project configuration
- `settings.json` - Runtime settings (auto-generated, stores blocked screen and hotkey preferences)

## Coding Conventions

### General Guidelines

1. **Minimal Changes**: Make the smallest possible changes to achieve the goal
2. **Preserve Functionality**: Don't modify or remove working code unless absolutely necessary
3. **Code Style**: Follow existing C# conventions in the codebase
4. **Comments**: Add comments only when necessary to explain complex logic
5. **Error Handling**: Use appropriate try-catch blocks for error-prone operations
6. **Localization**: Use the Strings class for user-facing text to support multiple languages

### C# Specific

- Use nullable reference types (enabled in project)
- Use implicit usings (enabled in project)
- Follow Microsoft C# naming conventions:
  - PascalCase for public members, methods, and properties
  - camelCase for private fields and local variables
  - UPPER_CASE for constants
- Use XML documentation comments for public APIs

## Building and Testing

### Build Commands

This is a Windows-specific application. Building requires Windows or Windows-targeting capabilities.

```bash
# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Publish single-file executable
dotnet publish --configuration Release --runtime win-x64 --output ./publish
```

### Testing

- The project currently has no automated test infrastructure
- Manual testing should be performed on Windows with multiple monitors
- Key features to verify:
  - Mouse blocking works on selected screen
  - System tray icon and context menu function correctly
  - Hotkey toggle (default: Ctrl+Alt+B) works
  - Settings persist across application restarts
  - Monitor names display correctly

## Key Features to Maintain

1. **Mouse Blocking**: Core functionality that prevents cursor from entering selected screen
2. **System Tray**: Runs in system tray with context menu for screen selection
3. **Global Hotkey**: Configurable hotkey to toggle blocking on/off
4. **Settings Persistence**: Saves blocked screen and hotkey to settings.json
5. **Localization**: Support for multiple languages (English, German, Spanish)
6. **Monitor Names**: Uses friendly monitor names from WMI when available
7. **Notifications**: Shows notification when mouse is blocked

## Common Tasks

### Adding New Features

- Consider internationalization - add strings to all Resources.*.resx files
- Update version number in MouseGuard.csproj if releasing
- Test with multiple monitor configurations

### Fixing Bugs

- Check existing issues and TODOs in README.md
- Ensure fixes work across different Windows versions
- Verify settings persistence isn't broken

### Modifying UI

- Use MainForm.Designer.cs for form designer changes
- Keep UI minimal and focused on system tray interaction
- Ensure localized strings are used for all UI text

## Platform Considerations

- **Windows-only**: This application uses Windows-specific APIs (user32.dll, System.Management for WMI)
- **Multi-Monitor**: Design decisions should consider various multi-monitor configurations
- **System Tray**: Application is designed to run in background with minimal UI
- **.NET 8.0**: Requires .NET 8.0 runtime on target systems (or self-contained deployment)

## CI/CD

- GitHub Actions workflow: `.github/workflows/release-on-merge.yml`
- Automatically builds and creates releases on merge to master
- Version is read from `MouseGuard.csproj`
- Creates a ZIP package of the published application

## Important Notes

- Do not modify working Win32 API interop code unless fixing a critical bug
- Preserve the timer-based mouse monitoring mechanism (20ms interval)
- Keep the application lightweight and performant
- Maintain backward compatibility with existing settings.json format
