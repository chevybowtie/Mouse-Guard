
# Todo

* Move settings to LocalAppData and ensure directory exists.
* Add icon fallback and dispose file-loaded icon on exit.
* Validate parsed hotkey has a key code before accepting.
* Check RegisterHotKey result and reflect failure (disable hotkey or notify).
* Dispose trayIcon and monitorTimer and close silentNotification on Exit.
* Avoid swallowing exceptions — log to a simple file or at least debug output.
* storing monitor DeviceName (or EDID) instead of screen index so user selection survives monitor reordering
* Replace broad catch { } with logging to a file in LocalAppData for later diagnostics.
* Move heavy operations (ManagementObjectSearcher) off the UI thread or cache results.
* Add explicit unit tests for hotkey parsing and settings read/write, and add a small logging mechanism to capture runtime failures.
