using System.Media;

namespace StretchReminder;

/// <summary>
/// The popup that appears each interval: shows the session's exercises one at a
/// time with a built-in hold timer, plus snooze/skip. Appears bottom-right,
/// topmost, without stealing keyboard focus.
/// </summary>
public class SessionForm : Form
{
    public SessionOutcome Outcome { get; private set; } = SessionOutcome.Skipped;

    private readonly Session _session;
    private readonly bool _lowImpact;
    private int _page;

    private readonly Label _lblSession;
    private readonly Label _lblProgress;
    private readonly Label _lblName;
    private readonly Label _lblTarget;
    private readonly Label _lblSteps;
    private readonly Label _lblDose;
    private readonly Label _lblCaution;
    private readonly Button _btnTimer;
    private readonly Button _btnNext;

    private readonly System.Windows.Forms.Timer _holdTimer;
    private int _holdRemaining;

    protected override bool ShowWithoutActivation => true;

    public SessionForm(Session session, int snoozeMinutes, string safetyFooter, Color accent, bool lowImpact = false)
    {
        _session = session;
        _lowImpact = lowImpact;
        var p = Theme.Current;

        // Suspend so the Dpi auto-scale pass runs once, after ClientSize and all
        // controls exist — otherwise it fires here with zero children and is consumed.
        SuspendLayout();

        Text = "Time to stretch";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        TopMost = true;
        BackColor = p.WindowBack;
        ClientSize = new Size(460, 576);
        HandleCreated += (_, _) => Theme.ApplyTitleBar(this);

        var wa = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        StartPosition = FormStartPosition.Manual;
        Location = new Point(wa.Right - Width - 16, wa.Bottom - Height - 16);

        _lblSession = new Label
        {
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = accent,
            Bounds = new Rectangle(20, 14, 300, 22),
            UseMnemonic = false, // session titles contain '&'
        };

        _lblProgress = new Label
        {
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = p.TextMuted,
            TextAlign = ContentAlignment.TopRight,
            Bounds = new Rectangle(320, 14, 120, 22),
        };

        _lblName = new Label
        {
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = p.TextPrimary,
            Bounds = new Rectangle(20, 42, 420, 58),
        };

        _lblTarget = new Label
        {
            Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
            ForeColor = p.TextMuted,
            Bounds = new Rectangle(20, 102, 420, 22),
        };

        _lblSteps = new Label
        {
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = p.TextPrimary,
            Bounds = new Rectangle(20, 132, 420, 200),
        };

        _lblDose = new Label
        {
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = accent,
            Bounds = new Rectangle(20, 338, 420, 44),
        };

        _lblCaution = new Label
        {
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = p.Caution,
            Bounds = new Rectangle(20, 384, 420, 38),
        };

        _btnTimer = new Button
        {
            Bounds = new Rectangle(20, 428, 200, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = accent,
            ForeColor = Color.White,
        };
        _btnTimer.FlatAppearance.BorderSize = 0;
        _btnTimer.Click += (_, _) => ToggleHoldTimer();

        _btnNext = new Button
        {
            Bounds = new Rectangle(300, 428, 140, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.ButtonSecondaryBack,
            ForeColor = p.ButtonSecondaryFore,
        };
        _btnNext.FlatAppearance.BorderColor = p.ButtonBorder;
        _btnNext.Click += (_, _) => NextOrFinish();

        var btnSnooze = new Button
        {
            Text = $"Snooze {snoozeMinutes} min",
            Bounds = new Rectangle(20, 478, 120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.WindowBack,
            ForeColor = p.TextMuted,
        };
        btnSnooze.FlatAppearance.BorderColor = p.ButtonBorder;
        btnSnooze.Click += (_, _) => CloseWith(SessionOutcome.Snoozed);

        var btnSkip = new Button
        {
            Text = "Skip",
            Bounds = new Rectangle(150, 478, 80, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.WindowBack,
            ForeColor = p.TextMuted,
        };
        btnSkip.FlatAppearance.BorderColor = p.ButtonBorder;
        btnSkip.Click += (_, _) => CloseWith(SessionOutcome.Skipped);

        var lblSafety = new Label
        {
            Text = safetyFooter,
            Font = new Font("Segoe UI", 8.25F),
            ForeColor = p.TextMuted,
            Bounds = new Rectangle(20, 518, 420, 52),
        };

        Controls.AddRange(new Control[]
        {
            _lblSession, _lblProgress, _lblName, _lblTarget, _lblSteps,
            _lblDose, _lblCaution, _btnTimer, _btnNext, btnSnooze, btnSkip, lblSafety,
        });

        // Esc snoozes once the popup has been clicked/focused. (It opens without
        // stealing focus, so keystrokes stay with whatever the user was doing.)
        CancelButton = btnSnooze;

        ResumeLayout(false);
        PerformLayout();

        _holdTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _holdTimer.Tick += (_, _) => OnHoldTick();

        ShowPage(0);
        SystemSounds.Asterisk.Play();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Re-anchor bottom-right after DPI auto-scaling has settled the final size.
        var wa = Screen.FromPoint(Location).WorkingArea;
        Location = new Point(wa.Right - Width - 16, wa.Bottom - Height - 16);
    }

    private Exercise Current
    {
        get
        {
            var ex = _session.Exercises[_page];
            return _lowImpact && ex.GentleVariant is not null ? ex.GentleVariant : ex;
        }
    }

    private void ShowPage(int page)
    {
        _page = page;
        StopHoldTimer();

        _lblSession.Text = _session.Title.ToUpperInvariant() + (_lowImpact ? "  ·  LOW-IMPACT" : "");
        _lblProgress.Text = $"Exercise {_page + 1} of {_session.Exercises.Length}";
        _lblName.Text = Current.Name;
        _lblTarget.Text = $"Targets: {Current.Target}";
        _lblSteps.Text = string.Join(Environment.NewLine,
            Current.Steps.Select(s => "•  " + s));
        _lblDose.Text = $"Do: {Current.Dose}";
        _lblCaution.Text = Current.Caution is null ? "" : "⚠ " + Current.Caution;
        _btnTimer.Text = $"Start {Current.TimerSeconds} s timer";
        _btnNext.Text = _page == _session.Exercises.Length - 1 ? "Finish  ✓" : "Next  →";
    }

    private void NextOrFinish()
    {
        if (_page == _session.Exercises.Length - 1)
            CloseWith(SessionOutcome.Completed);
        else
            ShowPage(_page + 1);
    }

    private void CloseWith(SessionOutcome outcome)
    {
        Outcome = outcome;
        Close();
    }

    // ---- hold timer ----------------------------------------------------------

    private void ToggleHoldTimer()
    {
        if (_holdTimer.Enabled)
        {
            StopHoldTimer();
            return;
        }
        _holdRemaining = Current.TimerSeconds;
        _btnTimer.Text = $"{_holdRemaining} s — tap to cancel";
        _holdTimer.Start();
    }

    private void OnHoldTick()
    {
        _holdRemaining--;
        if (_holdRemaining <= 0)
        {
            _holdTimer.Stop();
            SystemSounds.Exclamation.Play();
            _btnTimer.Text = "✓ Time!  Tap to run again";
            return;
        }
        _btnTimer.Text = $"{_holdRemaining} s — tap to cancel";
    }

    private void StopHoldTimer()
    {
        _holdTimer.Stop();
        _btnTimer.Text = $"Start {Current.TimerSeconds} s timer";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _holdTimer.Dispose();
        base.Dispose(disposing);
    }
}
