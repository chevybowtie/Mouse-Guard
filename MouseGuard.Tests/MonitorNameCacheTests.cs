namespace MouseGuard.Tests;

public class MonitorNameCacheTests
{
    [Fact]
    public void GetFriendlyName_CachesResolverResult()
    {
        var calls = 0;
        var cache = new MonitorNameCache(deviceName =>
        {
            calls++;
            return $"friendly-{deviceName}";
        });

        var first = cache.GetFriendlyName(@"\\.\DISPLAY1");
        var second = cache.GetFriendlyName(@"\\.\DISPLAY1");

        Assert.Equal(@"friendly-\\.\DISPLAY1", first);
        Assert.Equal(first, second);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Clear_RemovesCachedValues()
    {
        var calls = 0;
        var cache = new MonitorNameCache(deviceName =>
        {
            calls++;
            return $"friendly-{calls}-{deviceName}";
        });

        var first = cache.GetFriendlyName(@"\\.\DISPLAY1");
        cache.Clear();
        var second = cache.GetFriendlyName(@"\\.\DISPLAY1");

        Assert.NotEqual(first, second);
        Assert.Equal(2, calls);
    }
}
