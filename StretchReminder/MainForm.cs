namespace StretchReminder;

/// <summary>
/// Small status window: countdown to the next session, start/pause,
/// stretch-now, program/interval pickers, BMI row, water toggle.
/// Closing or minimizing hides it to the tray.
/// </summary>
public class MainForm : Form
{
    private readonly TrayAppContext _ctx;
    private readonly Label _lblCountdown;
    private readonly Label _lblNext;
    private readonly Button _btnPause;
    private readonly Button _btnNow;
    private readonly ComboBox _comboProgram;
    private readonly ComboBox _comboInterval;
    private readonly Label _lblBmi;
    private readonly CheckBox _chkWater;
    private bool _updatingUi;

    /// <summary>Set by the tray context on real exit so FormClosing stops intercepting.</summary>
    public bool AllowClose { get; set; }

    public MainForm(TrayAppContext ctx)
    {
        _ctx = ctx;
        var p = Theme.Current;

        // Suspend so the Dpi auto-scale pass runs once, after ClientSize and all
        // controls exist — otherwise it fires here with zero children and is consumed.
        SuspendLayout();

        Text = "Stretch Reminder";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(380, 360);
        BackColor = p.WindowBack;
        HandleCreated += (_, _) => Theme.ApplyTitleBar(this);

        _lblCountdown = new Label
        {
            Text = "--:--",
            Font = new Font("Segoe UI", 30F, FontStyle.Bold),
            ForeColor = p.AccentStrong,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(0, 18, 380, 56),
        };

        _lblNext = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10F),
            ForeColor = p.TextSecondary,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(0, 78, 380, 24),
            UseMnemonic = false, // session titles contain '&'
        };

        _btnPause = new Button
        {
            Text = "Pause",
            Bounds = new Rectangle(24, 118, 104, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.AccentTeal,
            ForeColor = Color.White,
        };
        _btnPause.FlatAppearance.BorderSize = 0;
        _btnPause.Click += (_, _) => _ctx.TogglePause();

        _btnNow = new Button
        {
            Text = "Stretch now",
            Bounds = new Rectangle(138, 118, 104, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.ButtonSecondaryBack,
            ForeColor = p.ButtonSecondaryFore,
        };
        _btnNow.FlatAppearance.BorderColor = p.ButtonBorder;
        _btnNow.Click += (_, _) => _ctx.StretchNow();

        var btnExit = new Button
        {
            Text = "Exit",
            Bounds = new Rectangle(252, 118, 104, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.ButtonSecondaryBack,
            ForeColor = p.Danger,
        };
        btnExit.FlatAppearance.BorderColor = p.ButtonBorder;
        btnExit.Click += (_, _) => _ctx.ExitApp();

        var lblProgram = new Label
        {
            Text = "Program:",
            Bounds = new Rectangle(48, 178, 105, 26),
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = p.TextSecondary,
        };

        _comboProgram = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds = new Rectangle(162, 177, 170, 28),
            BackColor = p.FieldBack,
            ForeColor = p.TextPrimary,
        };
        _comboProgram.Items.AddRange(new object[] { "Stretch", "Weight loss", "Mix (both)" });
        _comboProgram.SelectedIndexChanged += (_, _) =>
        {
            if (_updatingUi || _comboProgram.SelectedIndex < 0) return;
            _ctx.SetProgram((ProgramKind)_comboProgram.SelectedIndex);
        };

        var lblInterval = new Label
        {
            Text = "Remind every:",
            Bounds = new Rectangle(48, 214, 105, 26),
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = p.TextSecondary,
        };

        _comboInterval = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Bounds = new Rectangle(162, 213, 170, 28),
            BackColor = p.FieldBack,
            ForeColor = p.TextPrimary,
        };
        foreach (var m in TrayAppContext.IntervalChoices)
            _comboInterval.Items.Add($"{m} min");
        _comboInterval.SelectedIndexChanged += (_, _) =>
        {
            if (_updatingUi || _comboInterval.SelectedIndex < 0) return;
            _ctx.SetInterval(TrayAppContext.IntervalChoices[_comboInterval.SelectedIndex]);
        };

        _lblBmi = new Label
        {
            Text = "BMI: not set",
            Bounds = new Rectangle(24, 252, 216, 28),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = p.TextSecondary,
        };

        var btnProfile = new Button
        {
            Text = "Height/weight…",
            Bounds = new Rectangle(244, 250, 116, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.ButtonSecondaryBack,
            ForeColor = p.ButtonSecondaryFore,
            Font = new Font("Segoe UI", 9F),
        };
        btnProfile.FlatAppearance.BorderColor = p.ButtonBorder;
        btnProfile.Click += (_, _) => _ctx.OpenProfile();

        _chkWater = new CheckBox
        {
            Text = "Water reminder — a glass every hour",
            Bounds = new Rectangle(28, 290, 330, 24),
            ForeColor = p.TextSecondary,
        };
        _chkWater.CheckedChanged += (_, _) =>
        {
            if (_updatingUi) return;
            _ctx.SetWaterEnabled(_chkWater.Checked);
        };

        var lblHint = new Label
        {
            Text = "Close or minimize hides to the tray — Exit quits fully.",
            Font = new Font("Segoe UI", 8.25F),
            ForeColor = p.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(0, 328, 380, 20),
        };

        Controls.AddRange(new Control[]
        {
            _lblCountdown, _lblNext, _btnPause, _btnNow, btnExit,
            lblProgram, _comboProgram, lblInterval, _comboInterval,
            _lblBmi, btnProfile, _chkWater, lblHint,
        });

        ResumeLayout(false);
        PerformLayout();
    }

    public void UpdateStatus(TimeSpan remaining, bool paused, bool sessionOpen, string nextTitle, int intervalMinutes, ProgramKind program)
    {
        _updatingUi = true;
        try
        {
            _lblCountdown.Text = sessionOpen
                ? "Now!"
                : $"{(int)remaining.TotalMinutes}:{remaining.Seconds:00}";

            _lblNext.Text = sessionOpen
                ? "Session in progress — go!"
                : paused
                    ? $"Paused — next up: {nextTitle}"
                    : $"Next up: {nextTitle}";

            _btnPause.Text = paused ? "Resume" : "Pause";

            int recommended = TrayAppContext.RecommendedInterval(program);
            for (int i = 0; i < TrayAppContext.IntervalChoices.Length; i++)
            {
                int m = TrayAppContext.IntervalChoices[i];
                string text = m == recommended ? $"{m} min — recommended" : $"{m} min";
                if ((string)_comboInterval.Items[i]! != text)
                    _comboInterval.Items[i] = text;
            }

            int idx = Array.IndexOf(TrayAppContext.IntervalChoices, intervalMinutes);
            if (idx >= 0 && _comboInterval.SelectedIndex != idx)
                _comboInterval.SelectedIndex = idx;

            if (_comboProgram.SelectedIndex != (int)program)
                _comboProgram.SelectedIndex = (int)program;

            _lblBmi.Text = _ctx.BmiDisplay;
            if (_chkWater.Checked != _ctx.WaterEnabled)
                _chkWater.Checked = _ctx.WaterEnabled;
        }
        finally
        {
            _updatingUi = false;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Hide instead of closing for everything except a real shutdown/logoff
        // (which must never be vetoed) — a disposed form would break tray "Open".
        if (!AllowClose && e.CloseReason != CloseReason.WindowsShutDown)
        {
            e.Cancel = true;
            Hide();
            _ctx.NotifyMainHidden();
            return;
        }
        base.OnFormClosing(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            WindowState = FormWindowState.Normal; // so the next Show() restores normally
            _ctx.NotifyMainHidden();
        }
    }
}
