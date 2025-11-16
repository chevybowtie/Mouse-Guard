using System.Windows.Forms;

namespace MouseGuard.Tests;

public class HotkeyUtilTests
{
    [Fact]
    public void ToString_Roundtrip_ControlAltB()
    {
        var keys = Keys.Control | Keys.Alt | Keys.B;
        var s = HotkeyUtil.ToString(keys);
        Assert.Equal("Control,Alt,B", s);
        Assert.True(HotkeyUtil.TryParse(s, out var parsed));
        Assert.Equal(keys, parsed);
    }

    [Fact]
    public void TryParse_CaseInsensitive()
    {
        Assert.True(HotkeyUtil.TryParse("control,alt,b", out var k));
        Assert.Equal(Keys.Control | Keys.Alt | Keys.B, k);
    }

    [Fact]
    public void Parse_Invalid_ReturnsNone()
    {
        var k = HotkeyUtil.Parse("notakey");
        Assert.Equal(Keys.None, k);
    }
}
