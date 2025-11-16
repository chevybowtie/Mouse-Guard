using System;
using System.Linq;

namespace MouseGuard
{
    internal static class DisplayUtil
    {
        public static bool DeviceNameContainsInstanceToken(string deviceName, string instanceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName) || string.IsNullOrWhiteSpace(instanceName))
                return false;
            var tokens = instanceName.Split('\\');
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token)) continue;
                if (deviceName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        public static string ComposeDisplayName(string deviceName, string? friendlyName)
        {
            return string.IsNullOrEmpty(friendlyName) ? deviceName : $"{friendlyName} ({deviceName})";
        }

        public static string UserFriendlyNameFromUShorts(ushort[]? arr)
        {
            if (arr == null || arr.Length == 0) return string.Empty;
            var chars = arr.TakeWhile(v => v != 0).Select(v => (char)v).ToArray();
            return new string(chars);
        }
    }
}
