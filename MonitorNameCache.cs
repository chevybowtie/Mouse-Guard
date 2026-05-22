namespace MouseGuard;

internal sealed class MonitorNameCache
{
    private readonly Func<string, string?> _resolver;
    private readonly Dictionary<string, string?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public MonitorNameCache(Func<string, string?> resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    public string? GetFriendlyName(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            return null;

        if (_cache.TryGetValue(deviceName, out var cachedFriendlyName))
            return cachedFriendlyName;

        var resolvedFriendlyName = _resolver(deviceName);
        _cache[deviceName] = resolvedFriendlyName;
        return resolvedFriendlyName;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
