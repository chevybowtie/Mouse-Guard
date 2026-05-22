namespace MouseGuard.Tests;

public class CursorVisibilityControllerTests
{
    [Fact]
    public void EnsureHidden_OnlyCallsSetterOncePerStateTransition()
    {
        var transitions = new List<bool>();
        var controller = new CursorVisibilityController(visible => transitions.Add(visible));

        controller.EnsureHidden();
        controller.EnsureHidden();
        controller.EnsureVisible();
        controller.EnsureVisible();

        Assert.Equal(new[] { false, true }, transitions);
        Assert.True(controller.IsVisible);
    }

    [Fact]
    public void Constructor_WithNullSetter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CursorVisibilityController(null!));
    }
}
