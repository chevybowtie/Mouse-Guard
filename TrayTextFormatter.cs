using System.Windows.Forms;

namespace MouseGuard
{
    internal static class TrayTextFormatter
    {
        public static string Format(string trayName, bool blockingEnabled, Keys hotkey)
        {
            var status = blockingEnabled ? "Blocking" : "Unblocked";
            return $"{trayName} ({status}) - Hotkey: {HotkeyUtil.ToString(hotkey)}";
        }
    }
}
