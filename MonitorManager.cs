namespace MouseGuard;

/// <summary>
/// Manages monitor detection and single-monitor mode state.
/// Provides testable logic for monitor count checks and state transitions.
/// </summary>
public class MonitorManager
{
    private readonly IScreenProvider _screenProvider;
    private bool _isSingleMonitorMode;

    /// <summary>
    /// Gets whether the system is currently in single-monitor mode.
    /// </summary>
    public bool IsSingleMonitorMode => _isSingleMonitorMode;

    /// <summary>
    /// Event raised when transitioning from multi-monitor to single-monitor mode.
    /// </summary>
    public event EventHandler? TransitionedToSingleMonitor;

    /// <summary>
    /// Event raised when transitioning from single-monitor to multi-monitor mode.
    /// </summary>
    public event EventHandler? TransitionedToMultiMonitor;

    /// <summary>
    /// Creates a new MonitorManager with the specified screen provider.
    /// </summary>
    /// <param name="screenProvider">Provider for accessing screen information.</param>
    public MonitorManager(IScreenProvider screenProvider)
    {
        _screenProvider = screenProvider ?? throw new ArgumentNullException(nameof(screenProvider));
        _isSingleMonitorMode = GetMonitorCount() <= 1;
    }

    /// <summary>
    /// Gets the current monitor count.
    /// </summary>
    public int GetMonitorCount()
    {
        return _screenProvider.GetScreenCount();
    }

    /// <summary>
    /// Checks if the monitor count has changed and updates state accordingly.
    /// Returns true if a state transition occurred.
    /// </summary>
    public bool CheckForMonitorCountChange()
    {
        int monitorCount = GetMonitorCount();
        bool wasSingleMonitorMode = _isSingleMonitorMode;
        _isSingleMonitorMode = monitorCount <= 1;

        // Check for state transitions
        if (_isSingleMonitorMode && !wasSingleMonitorMode)
        {
            // Transitioned from multi to single
            TransitionedToSingleMonitor?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else if (!_isSingleMonitorMode && wasSingleMonitorMode)
        {
            // Transitioned from single to multi
            TransitionedToMultiMonitor?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if monitoring timer should be running based on current mode.
    /// Timer should run in single-monitor mode to detect when monitors are added.
    /// </summary>
    public bool ShouldMonitorCountTimerBeRunning()
    {
        return _isSingleMonitorMode;
    }
}

/// <summary>
/// Interface for accessing screen information (allows mocking for tests).
/// </summary>
public interface IScreenProvider
{
    /// <summary>
    /// Gets the number of screens currently connected.
    /// </summary>
    int GetScreenCount();
}

/// <summary>
/// Production implementation that uses System.Windows.Forms.Screen.
/// </summary>
public class WindowsFormsScreenProvider : IScreenProvider
{
    public int GetScreenCount()
    {
        return Screen.AllScreens.Length;
    }
}
