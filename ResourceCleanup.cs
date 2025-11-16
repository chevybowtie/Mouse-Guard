using System;
using System.Drawing;
using System.Windows.Forms;

namespace MouseGuard
{
    internal static class ResourceCleanup
    {
        public static void Cleanup(ref System.Windows.Forms.Timer? monitorTimer,
                                   ref System.Windows.Forms.Timer? monitorCountTimer,
                                   ref NotifyIcon? trayIcon,
                                   ref Program.SilentNotification? silentNotification,
                                   ref Icon? loadedIcon,
                                   ref SingleInstance? singleInstance,
                                   ref Program.HiddenMessageWindow? messageWindow)
        {
            // Stop and dispose timers
            if (monitorTimer != null)
            {
                try { monitorTimer.Stop(); } catch { }
                try { monitorTimer.Dispose(); } catch { }
                monitorTimer = null;
            }

            if (monitorCountTimer != null)
            {
                try { monitorCountTimer.Stop(); } catch { }
                try { monitorCountTimer.Dispose(); } catch { }
                monitorCountTimer = null;
            }

            // Close notification
            if (silentNotification != null)
            {
                try { silentNotification.Close(); } catch { }
                silentNotification = null;
            }

            // Dispose tray icon
            if (trayIcon != null)
            {
                try { trayIcon.Visible = false; } catch { }
                try { trayIcon.Dispose(); } catch { }
                trayIcon = null;
            }

            // Dispose loaded icon
            if (loadedIcon != null)
            {
                try { loadedIcon.Dispose(); } catch { }
                loadedIcon = null;
            }

            // Single instance dispose
            try { singleInstance?.Dispose(); } catch { }
            singleInstance = null;

            // Dispose message window
            try { messageWindow?.Close(); messageWindow?.Dispose(); } catch { }
            messageWindow = null;
        }
    }
}
