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
            if (TryGetKeyCode(keys, out var keyCode))
                parts.Add(keyCode.ToString());
            return string.Join(",", parts);
        }

        public static bool HasKeyCode(Keys keys)
        {
            return TryGetKeyCode(keys, out _);
        }

        public static bool TryGetKeyCode(Keys keys, out Keys keyCode)
        {
            keyCode = NormalizeKeyCode(keys);
            return keyCode != Keys.None;
        }

        public static bool TryParse(string s, out Keys keys)
        {
            keys = Keys.None;
            if (string.IsNullOrWhiteSpace(s)) return false;
            var parts = s.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
            Keys modifiers = Keys.None;
            Keys keyCode = Keys.None;

            foreach (var p in parts)
            {
                if (!Enum.TryParse<Keys>(p, ignoreCase: true, out var parsed))
                    return false;

                if (TryNormalizeModifier(parsed, out var modifier))
                {
                    modifiers |= modifier;
                    continue;
                }

                var normalizedKeyCode = NormalizeKeyCode(parsed);
                if (normalizedKeyCode == Keys.None)
                    return false;

                if (keyCode != Keys.None && keyCode != normalizedKeyCode)
                    return false;

                keyCode = normalizedKeyCode;
            }

            if (keyCode == Keys.None)
                return false;

            keys = modifiers | keyCode;
            return true;
        }

        public static Keys Parse(string s)
        {
            if (TryParse(s, out var k)) return k;
            return Keys.None;
        }

        private static Keys NormalizeKeyCode(Keys keys)
        {
            var keyCode = keys & Keys.KeyCode;
            return IsModifierKeyCode(keyCode) ? Keys.None : keyCode;
        }

        private static bool TryNormalizeModifier(Keys key, out Keys modifier)
        {
            switch (key)
            {
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    modifier = Keys.Control;
                    return true;
                case Keys.Alt:
                case Keys.Menu:
                case Keys.LMenu:
                case Keys.RMenu:
                    modifier = Keys.Alt;
                    return true;
                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    modifier = Keys.Shift;
                    return true;
                default:
                    modifier = Keys.None;
                    return false;
            }
        }

        private static bool IsModifierKeyCode(Keys keyCode)
        {
            return keyCode is Keys.None
                or Keys.ControlKey
                or Keys.ShiftKey
                or Keys.Menu
                or Keys.LControlKey
                or Keys.RControlKey
                or Keys.LShiftKey
                or Keys.RShiftKey
                or Keys.LMenu
                or Keys.RMenu;
        }
    }
}
