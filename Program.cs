using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;
using MouseGuard;
using System.Management;

/// <summary>
/// Mouse Guard application: blocks mouse from entering a selected screen.
/// </summary>
class Program
{
    // Mouse check interval in milliseconds
    private const int TimerIntervalMs = 20;
    // Monitor count check interval in milliseconds
    private const int MonitorCheckIntervalMs = 5000;
    // Default/invalid screen index
    private const int InvalidScreenIndex = -1;
    // Notification display duration in milliseconds
    private const int NotificationDurationMs = 3000;

    // Import SetCursorPos from user32.dll to move the mouse cursor
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);

    // Import ShowCursor from user32.dll to show/hide the mouse cursor
    [DllImport("user32.dll")]
    static extern bool ShowCursor(bool bShow);

    // System tray icon
    static NotifyIcon? trayIcon;
    // Timer for monitoring mouse position
    static System.Windows.Forms.Timer? monitorTimer;
    // Timer for checking monitor count changes
    static System.Windows.Forms.Timer? monitorCountTimer;
    // Index of the blocked screen (null if none)
    static int? blockedScreenIndex = null;
    // Monitor manager for handling single/multi-monitor detection
    static MonitorManager? monitorManager;

    // Hotkey constants
    private const int WM_HOTKEY = 0x0312;
    private const int HOTKEY_ID = 0xBEEF;
    private static Keys defaultHotkey = Keys.B | Keys.Control | Keys.Alt;
    private static Keys currentHotkey = defaultHotkey;
    private static bool blockingEnabled = true;

    // Notification flag
    private static bool hasShownBlockNotification = false;
    private static SilentNotification? silentNotification = null;

    /// <summary>
    /// Settings data structure for serialization.
    /// </summary>
    class AppSettings
    {
        public int BlockedScreenIndex { get; set; } = InvalidScreenIndex;
        public string? Hotkey { get; set; } // e.g. "Control,Alt,B"
    }

    /// <summary>
    /// Loads settings from disk or returns defaults.
    /// </summary>
    static AppSettings LoadSettings()
    {
        var settings = SettingsManager.LoadSettings<AppSettings>() ?? new AppSettings();
        if (!string.IsNullOrEmpty(settings.Hotkey))
        {
            if (TryParseHotkey(settings.Hotkey, out var k))
                currentHotkey = k;
        }
        return settings;
    }

    /// <summary>
    /// Saves current settings to disk.
    /// </summary>
    static void SaveSettings()
    {
        var settings = new AppSettings
        {
            BlockedScreenIndex = blockedScreenIndex ?? InvalidScreenIndex,
            Hotkey = HotkeyToString(currentHotkey)
        };
        SettingsManager.SaveSettings(settings);
    }

    /// <summary>
    /// Application entry point.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Migrate old settings from app directory if needed
        SettingsManager.MigrateOldSettings();

        // Load settings and set blocked screen if valid
        var settings = LoadSettings();
        if (settings.BlockedScreenIndex >= 0 && settings.BlockedScreenIndex < Screen.AllScreens.Length)
            blockedScreenIndex = settings.BlockedScreenIndex;
        else
            blockedScreenIndex = null;

        // Initialize monitor manager
        monitorManager = new MonitorManager(new WindowsFormsScreenProvider());
        
        // Set up event handlers for monitor count changes
        monitorManager.TransitionedToSingleMonitor += OnTransitionedToSingleMonitor;
        monitorManager.TransitionedToMultiMonitor += OnTransitionedToMultiMonitor;

        // Check for single monitor mode and show warning if needed
        if (monitorManager.IsSingleMonitorMode)
        {
            ShowSingleMonitorWarning();
        }

        // Tray icon initialization
        trayIcon = new NotifyIcon
        {
            Icon = new Icon("MouseGuard.ico"),
            Text = Strings.TrayIconText,
            Visible = true,
            ContextMenuStrip = BuildContextMenu()
        };

        // Register global hotkey
        RegisterHotkey(currentHotkey);

        // Start monitoring mouse position
        monitorTimer = new System.Windows.Forms.Timer { Interval = TimerIntervalMs };
        monitorTimer.Tick += MonitorMouse;
        monitorTimer.Start();

        // Start monitoring monitor count changes only if in single-monitor mode
        monitorCountTimer = new System.Windows.Forms.Timer { Interval = MonitorCheckIntervalMs };
        monitorCountTimer.Tick += CheckMonitorCount;
        if (monitorManager.IsSingleMonitorMode)
        {
            monitorCountTimer.Start();
        }

        // Message loop for hotkey
        Application.AddMessageFilter(new HotkeyMessageFilter(OnHotkeyPressed));

        Application.Run();

        // Unregister hotkey on exit
        UnregisterHotkey();
    }

    /// <summary>
    /// Builds the context menu for the tray icon.
    /// </summary>
    static ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        // If in single-monitor mode, show a disabled info item
        if (monitorManager != null && monitorManager.IsSingleMonitorMode)
        {
            var infoItem = new ToolStripMenuItem(Strings.SingleMonitorModeMenuLabel)
            {
                Enabled = false
            };
            menu.Items.Add(infoItem);
            menu.Items.Add(new ToolStripSeparator());
        }

        var screens = Screen.AllScreens;
        for (int i = 0; i < screens.Length; i++)
        {
            var index = i;
            var screen = screens[i];
            // Get friendly monitor name if available
            string? monitorName = GetMonitorFriendlyName(screen.DeviceName);
            string displayName = monitorName != null
                ? $"{monitorName} ({screen.DeviceName})"
                : screen.DeviceName;

            // Context menu building
            var item = new ToolStripMenuItem(
                Strings.BlockScreenMenu(i + 1, screen.Primary ? Strings.Primary : "", displayName, screen.Bounds.Width, screen.Bounds.Height))
            {
                CheckOnClick = true,
                Checked = blockedScreenIndex == index
            };

            // Click handler for screen selection (toggle block/unblock)
            item.Click += (s, e) =>
            {
                if (blockedScreenIndex == index)
                {
                    // Unblock if already blocked
                    item.Checked = false;
                    blockedScreenIndex = null;
                }
                else
                {
                    // Block this screen, uncheck others
                    foreach (ToolStripMenuItem m in menu.Items.OfType<ToolStripMenuItem>())
                        m.Checked = false;
                    item.Checked = true;
                    blockedScreenIndex = index;
                }
                SaveSettings();

                // Immediately update the menu to reflect the new state
                // (rebuilds the menu so the checked state is always correct)
                trayIcon!.ContextMenuStrip = BuildContextMenu();
            };

            menu.Items.Add(item);
        }

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Strings.AdvancedSettings, null, (s, e) => ShowAdvancedSettings(menu));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("About", null, (s, e) => ShowAboutDialog());
        menu.Items.Add(Strings.Exit, null, (s, e) => Exit());

        // Add About option

        return menu;
    }

    private static void ShowAboutDialog()
    {
        var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "Unknown";
        var repo = "https://github.com/chevybowtie/Mouse-Guard";

        var aboutForm = new Form
        {
            Text = "About Mouse Guard",
            Width = 340,
            Height = 180,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterScreen,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblTitle = new Label
        {
            Text = "Mouse Guard",
            Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblVersion = new Label
        {
            Text = $"Version: {version}",
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var link = new LinkLabel
        {
            Text = repo,
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.MiddleCenter
        };
        link.Links.Add(0, repo.Length, repo);
        link.LinkClicked += (s, e) =>
        {
            if (e.Link?.LinkData != null)
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = e.Link.LinkData.ToString(),
                    UseShellExecute = true
                });
            }
        };

        var btnOK = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Dock = DockStyle.Bottom
        };

        aboutForm.Controls.Add(btnOK);
        aboutForm.Controls.Add(link);
        aboutForm.Controls.Add(lblVersion);
        aboutForm.Controls.Add(lblTitle);

        aboutForm.AcceptButton = btnOK;
        aboutForm.ShowDialog();
    }

    static void ShowAdvancedSettings(ContextMenuStrip menu)
    {
        using (var dlg = new HotkeyDialog(currentHotkey))
        {
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Unregister old hotkey, register new one
                UnregisterHotkey();
                currentHotkey = dlg.SelectedHotkey;
                RegisterHotkey(currentHotkey);
                SaveSettings();
                trayIcon!.Text = $"{Strings.TrayIconText} ({(blockingEnabled ? "Blocking" : "Unblocked")})\nHotkey: {HotkeyToString(currentHotkey)}";
            }
        }
    }

    static void OnHotkeyPressed()
    {
        blockingEnabled = !blockingEnabled;
        trayIcon!.Text = $"{Strings.TrayIconText} ({(blockingEnabled ? "Blocking" : "Unblocked")})\nHotkey: {HotkeyToString(currentHotkey)}";
    }

    /// <summary>
    /// Timer event: checks mouse position and blocks if on blocked screen.
    /// </summary>
    static void MonitorMouse(object? sender, EventArgs e)
    {
        // If in single-monitor mode, don't do anything
        if (monitorManager != null && monitorManager.IsSingleMonitorMode)
        {
            ShowCursor(true);
            hasShownBlockNotification = false;
            return;
        }

        if (!blockingEnabled)
        {
            ShowCursor(true);
            hasShownBlockNotification = false;
            // Do not close notification here
            // silentNotification?.Close();
            // silentNotification = null;
            return;
        }

        // If no screen is blocked, do nothing
        if (blockedScreenIndex == null || blockedScreenIndex >= Screen.AllScreens.Length)
        {
            // If blockedScreenIndex was valid but now out of bounds, monitors may have changed
            if (blockedScreenIndex != null && blockedScreenIndex >= Screen.AllScreens.Length)
            {
                // Trigger monitor count check
                CheckMonitorCount(null, EventArgs.Empty);
            }
            hasShownBlockNotification = false;
            // Do not close notification here
            // silentNotification?.Close();
            // silentNotification = null;
            return;
        }

        var screen = Screen.AllScreens[(int)blockedScreenIndex];
        var bounds = screen.Bounds;
        var cursorPos = Cursor.Position;

        if (bounds.Contains(cursorPos))
        {
            // Move cursor to center of primary screen and hide it
            var primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen != null)
            {
                var safe = primaryScreen.Bounds;
                int safeX = safe.X + safe.Width / 2;
                int safeY = safe.Y + safe.Height / 2;
                SetCursorPos(safeX, safeY);
                ShowCursor(false);

                // Notification usage
                if (!hasShownBlockNotification)
                {
                    silentNotification?.Close();
                    silentNotification = new SilentNotification(Strings.NotificationTitle, Strings.NotificationMessage);
                    silentNotification.ShowNearTray();
                    hasShownBlockNotification = true;
                }
            }
        }
        else
        {
            // Show cursor if not on blocked screen
            ShowCursor(true);
            hasShownBlockNotification = false;
            // Do not close notification here
            // silentNotification?.Close();
            // silentNotification = null;
        }
    }

    /// <summary>
    /// Shows a warning dialog when only one monitor is detected.
    /// </summary>
    static void ShowSingleMonitorWarning()
    {
        MessageBox.Show(
            Strings.SingleMonitorWarningMessage,
            Strings.SingleMonitorWarningTitle,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    /// <summary>
    /// Timer tick handler that checks for monitor count changes.
    /// </summary>
    static void CheckMonitorCount(object? sender, EventArgs e)
    {
        monitorManager?.CheckForMonitorCountChange();
    }

    /// <summary>
    /// Event handler for when the system transitions to single-monitor mode.
    /// </summary>
    static void OnTransitionedToSingleMonitor(object? sender, EventArgs e)
    {
        ShowSingleMonitorWarning();
        // Clear blocked screen since we can't block with only one monitor
        blockedScreenIndex = null;
        SaveSettings();
        // Rebuild menu to reflect the change
        if (trayIcon != null)
            trayIcon.ContextMenuStrip = BuildContextMenu();
        // Start the monitor count timer to detect when a second monitor is added
        monitorCountTimer?.Start();
    }

    /// <summary>
    /// Event handler for when the system transitions to multi-monitor mode.
    /// </summary>
    static void OnTransitionedToMultiMonitor(object? sender, EventArgs e)
    {
        // Rebuild menu to show available screens
        if (trayIcon != null)
            trayIcon.ContextMenuStrip = BuildContextMenu();
        // Stop the monitor count timer since we don't need it in multi-monitor mode
        monitorCountTimer?.Stop();
    }

    /// <summary>
    /// Cleanly exits the application.
    /// </summary>
    static void Exit()
    {
        UnregisterHotkey();
        monitorTimer?.Stop();
        monitorCountTimer?.Stop();
        if (trayIcon != null)
            trayIcon.Visible = false;
        Application.Exit();
    }

    /// <summary>
    /// Gets the monitor model name for a given device name (e.g., \\.\DISPLAY1).
    /// </summary>
    static string? GetMonitorFriendlyName(string deviceName)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI",
                "SELECT * FROM WmiMonitorID");

            foreach (ManagementObject mo in searcher.Get())
            {
                // InstanceName contains the display device name, e.g., DISPLAY1
                var instanceName = (string)mo["InstanceName"];
                if (deviceName.Contains(instanceName.Split('\\').Last()))
                {
                    var nameArray = (ushort[])mo["UserFriendlyName"];
                    var name = System.Text.Encoding.UTF8.GetString(nameArray.Select(b => (byte)b).ToArray());
                    return name;
                }
            }
        }
        catch
        {
            // Ignore errors and fallback to device name
        }
        return null;
    }

    // --- Hotkey registration helpers ---

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private static IntPtr mainWindowHandle => Process.GetCurrentProcess().MainWindowHandle;

    private static void RegisterHotkey(Keys keys)
    {
        UnregisterHotkey();
        var (mod, vk) = KeysToModifiersAndVk(keys);
        RegisterHotKey(IntPtr.Zero, HOTKEY_ID, mod, vk);
    }

    private static void UnregisterHotkey()
    {
        UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
    }

    private static (uint, uint) KeysToModifiersAndVk(Keys keys)
    {
        uint mod = 0;
        if (keys.HasFlag(Keys.Control)) mod |= 0x2;
        if (keys.HasFlag(Keys.Alt)) mod |= 0x1;
        if (keys.HasFlag(Keys.Shift)) mod |= 0x4;
        var vk = (uint)(keys & Keys.KeyCode);
        return (mod, vk);
    }

    private static string HotkeyToString(Keys keys)
    {
        var parts = new List<string>();
        if (keys.HasFlag(Keys.Control)) parts.Add("Control");
        if (keys.HasFlag(Keys.Alt)) parts.Add("Alt");
        if (keys.HasFlag(Keys.Shift)) parts.Add("Shift");
        parts.Add((keys & Keys.KeyCode).ToString());
        return string.Join(",", parts);
    }

    private static bool TryParseHotkey(string s, out Keys keys)
    {
        keys = Keys.None;
        var parts = s.Split(',');
        foreach (var p in parts)
        {
            if (Enum.TryParse<Keys>(p, out var k))
                keys |= k;
        }
        return keys != Keys.None;
    }

    // --- Message filter for hotkey ---
    class HotkeyMessageFilter : IMessageFilter
    {
        private readonly Action callback;
        public HotkeyMessageFilter(Action cb) { callback = cb; }
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                callback();
                return true;
            }
            return false;
        }
    }

    // --- Hotkey selection dialog ---
    class HotkeyDialog : Form
    {
        public Keys SelectedHotkey { get; private set; }
        private Label lbl;
        public HotkeyDialog(Keys initial)
        {
            Text = Strings.HotkeyDialogTitle;
            Width = 300; Height = 120;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            lbl = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(FontFamily.GenericSansSerif, 12) };
            Controls.Add(lbl);
            SelectedHotkey = initial;
            UpdateLabel();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); return; }
            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu) return;
            SelectedHotkey = (e.Modifiers | e.KeyCode);
            UpdateLabel();
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        private void UpdateLabel()
        {
            lbl.Text = Strings.HotkeyDialogPrompt(HotkeyToString(SelectedHotkey));
        }
    }

    // --- Silent notification form (no sound, no taskbar) ---
    class SilentNotification : Form
    {
        private System.Windows.Forms.Timer timer;
        public SilentNotification(string title, string message)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Width = 260;
            Height = 80;
            BackColor = Color.White;
            Opacity = 0.95;
            Padding = new Padding(12);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var msgLabel = new Label
            {
                Text = message,
                Font = new Font(FontFamily.GenericSansSerif, 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            };
            Controls.Add(msgLabel);
            Controls.Add(titleLabel);

            timer = new System.Windows.Forms.Timer { Interval = NotificationDurationMs };
            timer.Tick += (s, e) => { Close(); };
        }

        public void ShowNearTray()
        {
            // Try to show near bottom right (primary screen)
            var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 800, 600);
            Left = workingArea.Right - Width - 10;
            Top = workingArea.Bottom - Height - 10;
            Show();
            timer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Stop();
            timer.Dispose();
            base.OnFormClosed(e);
        }
    }
}