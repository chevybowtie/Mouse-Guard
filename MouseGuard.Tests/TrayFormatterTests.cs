using System.Windows.Forms;

namespace MouseGuard.Tests;

public class TrayFormatterTests
{
    [Fact]
    public void Format_IncludesStatusAndHotkey()
    {
        var text = TrayTextFormatter.Format("Mouse Guard", true, Keys.Control | Keys.Alt | Keys.B);
        Assert.Equal("Mouse Guard (Blocking) - Hotkey: Control,Alt,B", text);
    }

    [Fact]
    public void Format_UnblockedStatus()
    {
        var text = TrayTextFormatter.Format("Mouse Guard", false, Keys.Control | Keys.Alt | Keys.B);
        Assert.Equal("Mouse Guard (Unblocked) - Hotkey: Control,Alt,B", text);
    }
}
