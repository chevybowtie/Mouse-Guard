using System.Windows.Forms;

namespace MouseGuard
{
    internal static class HotkeyUtil
    {
        public static string ToString(Keys keys)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (keys.HasFlag(Keys.Control)) parts.Add("Control");
            if (keys.HasFlag(Keys.Alt)) parts.Add("Alt");
            if (keys.HasFlag(Keys.Shift)) parts.Add("Shift");
            parts.Add((keys & Keys.KeyCode).ToString());
            return string.Join(",", parts);
        }

        public static bool TryParse(string s, out Keys keys)
        {
            keys = Keys.None;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var parts = s.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                if (Enum.TryParse<Keys>(p.Trim(), ignoreCase: true, out var k))
                    keys |= k;
            }
            return keys != Keys.None;
        }

        public static Keys Parse(string s)
        {
            if (TryParse(s, out var k)) return k;
            return Keys.None;
        }
    }
}
