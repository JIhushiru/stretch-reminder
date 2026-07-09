using System.Drawing.Drawing2D;
using Microsoft.Win32;

namespace StretchReminder;

/// <summary>
/// Owns the tray icon, the interval countdown, and the lifecycle of the
/// main window and session popups.
/// </summary>
public class TrayAppContext : ApplicationContext
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "StretchReminder";

    public static readonly int[] IntervalChoices = { 20, 30, 45, 60, 90 };

    private readonly NotifyIcon _tray;
    private readonly System.Windows.Forms.Timer _tick;
    private readonly AppConfig _config;
    private readonly AppState _state;
    private MainForm _mainForm;
    private readonly Icon _appIcon;
    private readonly ToolStripMenuItem _menuPause;
    private readonly ToolStripMenuItem _menuInterval;
    private readonly ToolStripMenuItem _menuProgram;
    private readonly ToolStripMenuItem _menuWater;
    private readonly ToolStripMenuItem _menuTheme;
    private readonly ToolStripMenuItem _menuAutoStart;

    private DateTime? _nextDue;              // set while running (not paused, no session open)
    private TimeSpan _pausedRemaining;       // countdown snapshot while paused
    private bool _paused;
    private DateTime? _waterNextDue;         // independent of the session countdown
    private SessionForm? _activeSession;
    private bool _activeIsWorkout;           // which rotation the open session came from
    private bool _hideHintShown;

    public TrayAppContext()
    {
        _config = Storage.LoadConfig();
        _state = Storage.LoadState();
        Theme.Refresh(_config.Theme); // must precede any form creation
        _appIcon = CreateAppIcon();

        _menuPause = new ToolStripMenuItem("Pause", null, (_, _) => TogglePause());
        _menuInterval = new ToolStripMenuItem("Interval");
        foreach (var minutes in IntervalChoices)
        {
            var m = minutes; // capture
            _menuInterval.DropDownItems.Add(
                new ToolStripMenuItem($"{m} minutes", null, (_, _) => SetInterval(m)));
        }
        _menuProgram = new ToolStripMenuItem("Program");
        _menuProgram.DropDownItems.Add(
            new ToolStripMenuItem("Stretch — neck && posture", null, (_, _) => SetProgram(ProgramKind.Stretch)));
        _menuProgram.DropDownItems.Add(
            new ToolStripMenuItem("Weight loss — mini-workouts", null, (_, _) => SetProgram(ProgramKind.WeightLoss)));
        _menuProgram.DropDownItems.Add(
            new ToolStripMenuItem("Mix — alternate both", null, (_, _) => SetProgram(ProgramKind.Mix)));

        _menuWater = new ToolStripMenuItem("Water reminder (hourly)", null, (_, _) => SetWaterEnabled(!_config.WaterEnabled))
        {
            Checked = _config.WaterEnabled,
        };

        _menuTheme = new ToolStripMenuItem("Theme");
        _menuTheme.DropDownItems.Add(
            new ToolStripMenuItem("System default", null, (_, _) => SetTheme(ThemeKind.System)));
        _menuTheme.DropDownItems.Add(
            new ToolStripMenuItem("Light", null, (_, _) => SetTheme(ThemeKind.Light)));
        _menuTheme.DropDownItems.Add(
            new ToolStripMenuItem("Dark", null, (_, _) => SetTheme(ThemeKind.Dark)));

        _menuAutoStart = new ToolStripMenuItem("Start with Windows", null, (_, _) => ToggleAutoStart())
        {
            Checked = IsAutoStartEnabled(),
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Open", null, (_, _) => ShowMainWindow()));
        menu.Items.Add(new ToolStripMenuItem("Stretch now", null, (_, _) => StretchNow()));
        menu.Items.Add(_menuPause);
        menu.Items.Add(_menuProgram);
        menu.Items.Add(_menuInterval);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Height && weight…", null, (_, _) => OpenProfile()));
        menu.Items.Add(_menuWater);
        menu.Items.Add(_menuTheme);
        menu.Items.Add(_menuAutoStart);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitApp()));

        _tray = new NotifyIcon
        {
            Icon = _appIcon,
            Text = "Stretch Reminder",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _tray.DoubleClick += (_, _) => ShowMainWindow();

        _mainForm = new MainForm(this) { Icon = _appIcon };

        _tick = new System.Windows.Forms.Timer { Interval = 500 };
        _tick.Tick += (_, _) => OnTick();
        _tick.Start();

        StartCountdown(TimeSpan.FromMinutes(_config.IntervalMinutes));
        if (_config.WaterEnabled)
            _waterNextDue = DateTime.UtcNow + TimeSpan.FromMinutes(_config.WaterIntervalMinutes);
        UpdateIntervalChecks();
        UpdateProgramChecks();
        UpdateThemeChecks();
        _mainForm.Show();
        UpdateUi();
    }

    /// <summary>Interval that fits each program's dose (workouts need recovery between snacks).</summary>
    public static int RecommendedInterval(ProgramKind program) =>
        program == ProgramKind.WeightLoss ? 60 : 45;

    // ---- state the UI reads -------------------------------------------------

    public bool Paused => _paused;
    public int IntervalMinutes => _config.IntervalMinutes;
    public bool SessionOpen => _activeSession != null;
    public ProgramKind Program => _config.Program;
    public bool WaterEnabled => _config.WaterEnabled;
    public string NextSessionTitle => PeekNext().session.Title;

    public string BmiDisplay => _config.Bmi is double bmi
        ? $"BMI: {bmi:0.0} — {AppConfig.BmiCategory(bmi)}{(_config.LowImpact ? " (low-impact)" : "")}"
        : "BMI: not set";

    public TimeSpan Remaining
    {
        get
        {
            if (_paused) return _pausedRemaining;
            if (_nextDue is DateTime due)
            {
                var left = due - DateTime.UtcNow;
                return left > TimeSpan.Zero ? left : TimeSpan.Zero;
            }
            return TimeSpan.Zero;
        }
    }

    // ---- actions ------------------------------------------------------------

    public void TogglePause()
    {
        if (_paused)
        {
            _paused = false;
            if (_activeSession == null)
                _nextDue = DateTime.UtcNow + _pausedRemaining;
        }
        else
        {
            _pausedRemaining = Remaining;
            if (_pausedRemaining <= TimeSpan.Zero)
                _pausedRemaining = TimeSpan.FromMinutes(_config.IntervalMinutes);
            _paused = true;
            _nextDue = null;
        }
        // water rides along: fresh interval on resume, dormant while paused
        if (!_paused && _config.WaterEnabled)
            _waterNextDue = DateTime.UtcNow + TimeSpan.FromMinutes(_config.WaterIntervalMinutes);
        UpdateUi();
    }

    public void SetWaterEnabled(bool enabled)
    {
        _config.WaterEnabled = enabled;
        Storage.SaveConfig(_config);
        _waterNextDue = enabled ? DateTime.UtcNow + TimeSpan.FromMinutes(_config.WaterIntervalMinutes) : null;
        _menuWater.Checked = enabled;
        UpdateUi();
    }

    public void SetTheme(ThemeKind theme)
    {
        _config.Theme = theme;
        _config.Sanitize();
        Storage.SaveConfig(_config);
        Theme.Refresh(_config.Theme);
        UpdateThemeChecks();

        // Forms read the palette at construction — rebuild the main window now;
        // popups and dialogs pick it up when they're next created.
        bool wasVisible = !_mainForm.IsDisposed && _mainForm.Visible;
        if (!_mainForm.IsDisposed)
        {
            _mainForm.AllowClose = true;
            _mainForm.Dispose();
        }
        _mainForm = new MainForm(this) { Icon = _appIcon };
        if (wasVisible) _mainForm.Show();
        UpdateUi();
    }

    private void UpdateThemeChecks()
    {
        // items are added in enum order: System, Light, Dark
        for (int i = 0; i < _menuTheme.DropDownItems.Count; i++)
            ((ToolStripMenuItem)_menuTheme.DropDownItems[i]).Checked = i == (int)_config.Theme;
    }

    private ProfileForm? _profileDialog;

    public void OpenProfile()
    {
        if (_profileDialog is { IsDisposed: false })
        {
            _profileDialog.Activate(); // tray menu stays live during ShowDialog — don't stack a second dialog
            return;
        }

        using var dialog = new ProfileForm(_config.HeightCm, _config.WeightKg) { Icon = _appIcon };
        _profileDialog = dialog;
        try
        {
            var owner = !_mainForm.IsDisposed && _mainForm.Visible ? _mainForm : null;
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                _config.HeightCm = dialog.HeightCm;
                _config.WeightKg = dialog.WeightKg;
                _config.Sanitize();
                Storage.SaveConfig(_config);
                UpdateUi(); // low-impact mode applies from the next workout popup
            }
        }
        finally
        {
            _profileDialog = null;
        }
    }

    public void StretchNow()
    {
        if (_activeSession != null)
        {
            _activeSession.Activate();
            return;
        }
        ShowSession();
    }

    public void SetInterval(int minutes)
    {
        _config.IntervalMinutes = minutes;
        _config.Sanitize();
        Storage.SaveConfig(_config);
        if (_activeSession == null)
            StartCountdown(TimeSpan.FromMinutes(_config.IntervalMinutes));
        UpdateIntervalChecks();
        UpdateUi();
    }

    public void SetProgram(ProgramKind program)
    {
        _config.Program = program;
        _config.Sanitize();
        Storage.SaveConfig(_config);
        UpdateProgramChecks();
        UpdateIntervalChecks(); // the "(recommended)" mark follows the program
        UpdateUi(); // countdown keeps running; only the upcoming content changes
    }

    /// <summary>Called by MainForm when it hides itself to the tray.</summary>
    public void NotifyMainHidden()
    {
        if (_hideHintShown) return;
        _hideHintShown = true;
        _tray.ShowBalloonTip(2500, "Still running",
            "Stretch Reminder keeps running in the tray. Double-click the icon to open it.",
            ToolTipIcon.Info);
    }

    // ---- internals ----------------------------------------------------------

    private void OnTick()
    {
        if (!_paused && _activeSession == null && _nextDue is DateTime due && DateTime.UtcNow >= due)
            ShowSession();

        if (!_paused && _config.WaterEnabled && _waterNextDue is DateTime waterDue && DateTime.UtcNow >= waterDue)
        {
            _waterNextDue = DateTime.UtcNow + TimeSpan.FromMinutes(_config.WaterIntervalMinutes);
            _tray.ShowBalloonTip(5000, "Water break 💧",
                "Down a glass (~250 ml). Around 8 glasses spread across the day is plenty — more in the heat or after a workout.",
                ToolTipIcon.None);
        }

        UpdateUi();
    }

    private void StartCountdown(TimeSpan duration)
    {
        if (_paused)
        {
            _pausedRemaining = duration;
            _nextDue = null;
        }
        else
        {
            // UTC: immune to DST jumps and clock corrections while a countdown is pending
            _nextDue = DateTime.UtcNow + duration;
        }
    }

    /// <summary>The session the next popup will show, per the selected program.</summary>
    private (Session session, bool isWorkout) PeekNext() => _config.Program switch
    {
        ProgramKind.WeightLoss => (Exercises.WorkoutSessions[_state.WorkoutIndex], true),
        ProgramKind.Mix when _state.MixNextIsWorkout => (Exercises.WorkoutSessions[_state.WorkoutIndex], true),
        _ => (Exercises.StretchSessions[_state.StretchIndex], false),
    };

    private void ShowSession()
    {
        _nextDue = null;
        var (session, isWorkout) = PeekNext();
        _activeIsWorkout = isWorkout;
        _activeSession = new SessionForm(
            session,
            _config.SnoozeMinutes,
            isWorkout ? Exercises.WorkoutFooter : Exercises.SafetyFooter,
            isWorkout ? Theme.Current.AccentOrange : Theme.Current.AccentTeal,
            lowImpact: isWorkout && _config.LowImpact)
        { Icon = _appIcon };
        _activeSession.FormClosed += (_, _) =>
        {
            var outcome = _activeSession!.Outcome;
            _activeSession.Dispose();
            _activeSession = null;

            if (outcome == SessionOutcome.Snoozed)
            {
                // same session comes back after the snooze
                StartCountdown(TimeSpan.FromMinutes(_config.SnoozeMinutes));
            }
            else
            {
                if (_activeIsWorkout)
                    _state.WorkoutIndex = (_state.WorkoutIndex + 1) % Exercises.WorkoutSessions.Length;
                else
                    _state.StretchIndex = (_state.StretchIndex + 1) % Exercises.StretchSessions.Length;
                _state.MixNextIsWorkout = !_activeIsWorkout; // keeps Mix alternating
                Storage.SaveState(_state);
                StartCountdown(TimeSpan.FromMinutes(_config.IntervalMinutes));
            }
            UpdateUi();
        };
        _activeSession.Show();
        UpdateUi();
    }

    private void ShowMainWindow()
    {
        if (_mainForm.IsDisposed) // e.g. closed by a vetoed shutdown or external WM_CLOSE
            _mainForm = new MainForm(this) { Icon = _appIcon };
        _mainForm.Show();
        _mainForm.WindowState = FormWindowState.Normal;
        _mainForm.Activate();
    }

    private void UpdateUi()
    {
        if (!_mainForm.IsDisposed)
            _mainForm.UpdateStatus(Remaining, _paused, SessionOpen, NextSessionTitle, _config.IntervalMinutes, _config.Program);
        _menuPause.Text = _paused ? "Resume" : "Pause";

        string text = _paused
            ? "Stretch Reminder — paused"
            : SessionOpen
                ? "Stretch Reminder — session open"
                : $"Stretch Reminder — next in {FormatShort(Remaining)}";
        _tray.Text = text.Length <= 63 ? text : text[..63];
    }

    private void UpdateIntervalChecks()
    {
        int recommended = RecommendedInterval(_config.Program);
        for (int i = 0; i < IntervalChoices.Length; i++)
        {
            var item = (ToolStripMenuItem)_menuInterval.DropDownItems[i];
            item.Checked = IntervalChoices[i] == _config.IntervalMinutes;
            item.Text = IntervalChoices[i] == recommended
                ? $"{IntervalChoices[i]} minutes (recommended)"
                : $"{IntervalChoices[i]} minutes";
        }
    }

    private void UpdateProgramChecks()
    {
        // items are added in enum order: Stretch, WeightLoss, Mix
        for (int i = 0; i < _menuProgram.DropDownItems.Count; i++)
            ((ToolStripMenuItem)_menuProgram.DropDownItems[i]).Checked = i == (int)_config.Program;
    }

    private static string FormatShort(TimeSpan t) =>
        t.TotalMinutes >= 1 ? $"{(int)Math.Ceiling(t.TotalMinutes)} min" : $"{Math.Max(0, (int)t.TotalSeconds)} s";

    public void ExitApp()
    {
        _mainForm.AllowClose = true;
        _tick.Stop();
        _tray.Visible = false;
        _tray.Dispose();
        ExitThread();
    }

    // ---- autostart ----------------------------------------------------------

    private static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(RunValueName) != null;
        }
        catch { return false; }
    }

    private void ToggleAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (_menuAutoStart.Checked)
                key.DeleteValue(RunValueName, throwOnMissingValue: false);
            else
                key.SetValue(RunValueName, $"\"{Application.ExecutablePath}\"");
            _menuAutoStart.Checked = !_menuAutoStart.Checked;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Couldn't change the startup setting:\n{ex.Message}",
                "Stretch Reminder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ---- icon ---------------------------------------------------------------

    private static Icon CreateAppIcon()
    {
        // Prefer the multi-resolution icon embedded at build time (same app.ico the
        // exe/taskbar icon comes from), so all sizes render crisp.
        try
        {
            var stream = typeof(TrayAppContext).Assembly
                .GetManifestResourceStream("StretchReminder.app.ico");
            if (stream is not null)
                using (stream) return new Icon(stream);
        }
        catch { /* fall back to the drawn icon */ }

        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var fill = new SolidBrush(Color.FromArgb(0, 137, 123)); // teal
            g.FillEllipse(fill, 1, 1, 30, 30);
            using var font = new Font("Segoe UI", 16, FontStyle.Bold, GraphicsUnit.Pixel);
            var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("S", font, Brushes.White, new RectangleF(0, 0, 32, 31), fmt);
        }
        return Icon.FromHandle(bmp.GetHicon()); // one-time HICON; lives for the app's lifetime
    }
}
