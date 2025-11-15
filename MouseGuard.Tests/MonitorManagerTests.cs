namespace MouseGuard.Tests;

/// <summary>
/// Tests for the MonitorManager class to verify single/multi-monitor detection
/// and state transitions.
/// </summary>
public class MonitorManagerTests
{
    /// <summary>
    /// Mock screen provider for testing.
    /// </summary>
    private class MockScreenProvider : IScreenProvider
    {
        public int ScreenCount { get; set; }

        public int GetScreenCount() => ScreenCount;
    }

    [Fact]
    public void Constructor_WithOneMonitor_SetsSingleMonitorMode()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 1 };

        // Act
        var manager = new MonitorManager(mockProvider);

        // Assert
        Assert.True(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void Constructor_WithZeroMonitors_SetsSingleMonitorMode()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 0 };

        // Act
        var manager = new MonitorManager(mockProvider);

        // Assert
        Assert.True(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void Constructor_WithTwoMonitors_SetsMultiMonitorMode()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };

        // Act
        var manager = new MonitorManager(mockProvider);

        // Assert
        Assert.False(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void Constructor_WithThreeMonitors_SetsMultiMonitorMode()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 3 };

        // Act
        var manager = new MonitorManager(mockProvider);

        // Assert
        Assert.False(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void Constructor_WithNullProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MonitorManager(null!));
    }

    [Fact]
    public void GetMonitorCount_ReturnsCorrectCount()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);

        // Act
        var count = manager.GetMonitorCount();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void CheckForMonitorCountChange_NoChange_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);

        // Act
        var changed = manager.CheckForMonitorCountChange();

        // Assert
        Assert.False(changed);
        Assert.False(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void CheckForMonitorCountChange_FromMultiToSingle_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);
        mockProvider.ScreenCount = 1;

        // Act
        var changed = manager.CheckForMonitorCountChange();

        // Assert
        Assert.True(changed);
        Assert.True(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void CheckForMonitorCountChange_FromSingleToMulti_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 1 };
        var manager = new MonitorManager(mockProvider);
        mockProvider.ScreenCount = 2;

        // Act
        var changed = manager.CheckForMonitorCountChange();

        // Assert
        Assert.True(changed);
        Assert.False(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void TransitionedToSingleMonitor_EventRaisedWhenTransitioning()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);
        bool eventRaised = false;
        manager.TransitionedToSingleMonitor += (s, e) => eventRaised = true;
        mockProvider.ScreenCount = 1;

        // Act
        manager.CheckForMonitorCountChange();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void TransitionedToMultiMonitor_EventRaisedWhenTransitioning()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 1 };
        var manager = new MonitorManager(mockProvider);
        bool eventRaised = false;
        manager.TransitionedToMultiMonitor += (s, e) => eventRaised = true;
        mockProvider.ScreenCount = 2;

        // Act
        manager.CheckForMonitorCountChange();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void TransitionedToSingleMonitor_EventNotRaisedWhenNoChange()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 1 };
        var manager = new MonitorManager(mockProvider);
        bool eventRaised = false;
        manager.TransitionedToSingleMonitor += (s, e) => eventRaised = true;

        // Act
        manager.CheckForMonitorCountChange();

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void TransitionedToMultiMonitor_EventNotRaisedWhenNoChange()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);
        bool eventRaised = false;
        manager.TransitionedToMultiMonitor += (s, e) => eventRaised = true;

        // Act
        manager.CheckForMonitorCountChange();

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void ShouldMonitorCountTimerBeRunning_SingleMonitor_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 1 };
        var manager = new MonitorManager(mockProvider);

        // Act
        var shouldRun = manager.ShouldMonitorCountTimerBeRunning();

        // Assert
        Assert.True(shouldRun);
    }

    [Fact]
    public void ShouldMonitorCountTimerBeRunning_MultiMonitor_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);

        // Act
        var shouldRun = manager.ShouldMonitorCountTimerBeRunning();

        // Assert
        Assert.False(shouldRun);
    }

    [Fact]
    public void MultipleTransitions_StateTrackedCorrectly()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);
        int toSingleCount = 0;
        int toMultiCount = 0;
        manager.TransitionedToSingleMonitor += (s, e) => toSingleCount++;
        manager.TransitionedToMultiMonitor += (s, e) => toMultiCount++;

        // Act & Assert - Start with 2 monitors
        Assert.False(manager.IsSingleMonitorMode);

        // Transition to 1 monitor
        mockProvider.ScreenCount = 1;
        manager.CheckForMonitorCountChange();
        Assert.True(manager.IsSingleMonitorMode);
        Assert.Equal(1, toSingleCount);
        Assert.Equal(0, toMultiCount);

        // Transition back to 2 monitors
        mockProvider.ScreenCount = 2;
        manager.CheckForMonitorCountChange();
        Assert.False(manager.IsSingleMonitorMode);
        Assert.Equal(1, toSingleCount);
        Assert.Equal(1, toMultiCount);

        // Transition to 1 again
        mockProvider.ScreenCount = 1;
        manager.CheckForMonitorCountChange();
        Assert.True(manager.IsSingleMonitorMode);
        Assert.Equal(2, toSingleCount);
        Assert.Equal(1, toMultiCount);
    }

    [Fact]
    public void CheckForMonitorCountChange_From2To3Monitors_NoTransition()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 2 };
        var manager = new MonitorManager(mockProvider);
        bool eventRaised = false;
        manager.TransitionedToSingleMonitor += (s, e) => eventRaised = true;
        manager.TransitionedToMultiMonitor += (s, e) => eventRaised = true;
        mockProvider.ScreenCount = 3;

        // Act
        var changed = manager.CheckForMonitorCountChange();

        // Assert
        Assert.False(changed);
        Assert.False(eventRaised);
        Assert.False(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void CheckForMonitorCountChange_From3To1Monitors_TransitionOccurs()
    {
        // Arrange
        var mockProvider = new MockScreenProvider { ScreenCount = 3 };
        var manager = new MonitorManager(mockProvider);
        bool toSingleEventRaised = false;
        manager.TransitionedToSingleMonitor += (s, e) => toSingleEventRaised = true;
        mockProvider.ScreenCount = 1;

        // Act
        var changed = manager.CheckForMonitorCountChange();

        // Assert
        Assert.True(changed);
        Assert.True(toSingleEventRaised);
        Assert.True(manager.IsSingleMonitorMode);
    }

    [Fact]
    public void WindowsFormsScreenProvider_GetScreenCount_ReturnsPositiveNumber()
    {
        // Arrange
        var provider = new WindowsFormsScreenProvider();

        // Act
        var count = provider.GetScreenCount();

        // Assert
        Assert.True(count > 0, "Screen count should be at least 1 in any test environment");
    }
}
