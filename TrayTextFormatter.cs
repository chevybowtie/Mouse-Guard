using System.Windows.Forms;

namespace MouseGuard
{
    internal static class TrayTextFormatter
    {
        public static string Format(string trayName, bool blockingEnabled, Keys hotkey, bool hotkeyRegistered)
        {
            var status = blockingEnabled ? "Blocking" : "Unblocked";
            return hotkeyRegistered
                ? $"{trayName} ({status}) - Hotkey: {HotkeyUtil.ToString(hotkey)}"
                : $"{trayName} ({status}) - {Strings.HotkeyUnavailable}";
        }
    }
}
