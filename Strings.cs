using System.Globalization;
using System.Resources;

namespace MouseGuard
{
    internal static class Strings
    {
        private static readonly ResourceManager RM = new ResourceManager("MouseGuard.Resources", typeof(Strings).Assembly);
        public static string TrayIconText => RM.GetString("TrayIconText", CultureInfo.CurrentUICulture) ?? "Mouse Guard";
        public static string BlockScreenMenu(int idx, string primary, string displayName, int w, int h) =>
            string.Format(RM.GetString("BlockScreenMenu", CultureInfo.CurrentUICulture) ?? "Block Screen {0} {1} - {2} [{3}x{4}]", idx, primary, displayName, w, h);
        public static string Primary => RM.GetString("Primary", CultureInfo.CurrentUICulture) ?? "(Primary)";
        public static string AdvancedSettings => RM.GetString("AdvancedSettings", CultureInfo.CurrentUICulture) ?? "Advanced Settings...";
        public static string Exit => RM.GetString("Exit", CultureInfo.CurrentUICulture) ?? "Exit";
        public static string HotkeyDialogTitle => RM.GetString("HotkeyDialogTitle", CultureInfo.CurrentUICulture) ?? "Select Hotkey";
        public static string HotkeyDialogPrompt(string hotkey) =>
            string.Format(RM.GetString("HotkeyDialogPrompt", CultureInfo.CurrentUICulture) ?? "Press new hotkey (Esc=Cancel, Enter=OK):\n{0}", hotkey);
        public static string NotificationTitle => RM.GetString("NotificationTitle", CultureInfo.CurrentUICulture) ?? "Mouse Blocked";
        public static string NotificationMessage => RM.GetString("NotificationMessage", CultureInfo.CurrentUICulture) ?? "The mouse was blocked from entering the selected screen.";
    }
}
