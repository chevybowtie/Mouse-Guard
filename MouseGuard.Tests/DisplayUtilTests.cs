namespace MouseGuard.Tests;

public class DisplayUtilTests
{
    [Fact]
    public void DeviceNameContainsInstanceToken_MatchesTokenInDeviceName()
    {
        var device = @"\\.\DISPLAY1";
        var instance = "DISPLAY1\\5&1a2b3c4d&0&UID4353";
        Assert.True(DisplayUtil.DeviceNameContainsInstanceToken(device, instance));
    }

    [Fact]
    public void DeviceNameContainsInstanceToken_NoMatch_ReturnsFalse()
    {
        var device = @"\\.\DISPLAY2";
        var instance = "DISPLAY1\\foo";
        Assert.False(DisplayUtil.DeviceNameContainsInstanceToken(device, instance));
    }

    [Fact]
    public void ComposeDisplayName_WithFriendly_IncludesBoth()
    {
        var text = DisplayUtil.ComposeDisplayName(@"\\.\DISPLAY1", "DELL U2720Q");
        Assert.Equal(@"DELL U2720Q (\\.\DISPLAY1)", text);
    }

    [Fact]
    public void ComposeDisplayName_WithoutFriendly_UsesDevice()
    {
        var text = DisplayUtil.ComposeDisplayName(@"\\.\DISPLAY1", null);
        Assert.Equal(@"\\.\DISPLAY1", text);
    }

    [Fact]
    public void UserFriendlyNameFromUShorts_StopsAtNull()
    {
        ushort[] data = new ushort[] { 'D', 'E', 'L', 'L', 0, 'X' };
        var result = DisplayUtil.UserFriendlyNameFromUShorts(data);
        Assert.Equal("DELL", result);
    }
}
