namespace MouseGuard;

internal sealed class CursorVisibilityController
{
    private readonly Action<bool> _setCursorVisible;

    public CursorVisibilityController(Action<bool> setCursorVisible)
    {
        _setCursorVisible = setCursorVisible ?? throw new ArgumentNullException(nameof(setCursorVisible));
    }

    public bool IsVisible { get; private set; } = true;

    public void EnsureVisible()
    {
        SetVisibility(true);
    }

    public void EnsureHidden()
    {
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        if (IsVisible == visible)
            return;

        _setCursorVisible(visible);
        IsVisible = visible;
    }
}
