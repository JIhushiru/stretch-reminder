namespace StretchReminder;

/// <summary>
/// Small dialog to pick the reminder sound: toggle it on/off, choose a custom
/// .wav, preview it, or fall back to the bundled default chime.
/// </summary>
public class SoundForm : Form
{
    /// <summary>Whether reminders should play a sound.</summary>
    public bool SoundEnabled => _chkEnabled.Checked;

    /// <summary>Chosen .wav path, or "" for the bundled default.</summary>
    public string SoundFilePath { get; private set; }

    private readonly CheckBox _chkEnabled;
    private readonly Label _lblCurrent;
    private readonly Button _btnPreview;
    private readonly Button _btnReset;

    public SoundForm(bool enabled, string soundFilePath)
    {
        SoundFilePath = soundFilePath ?? "";
        var p = Theme.Current;

        SuspendLayout();

        Text = "Reminder sound";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(400, 230);
        BackColor = p.WindowBack;
        HandleCreated += (_, _) => Theme.ApplyTitleBar(this);

        _chkEnabled = new CheckBox
        {
            Text = "Play a sound when a reminder appears",
            Checked = enabled,
            Bounds = new Rectangle(24, 22, 352, 24),
            ForeColor = p.TextPrimary,
        };
        _chkEnabled.CheckedChanged += (_, _) => SyncEnabledState();

        var lblLabel = new Label
        {
            Text = "Current sound:",
            Bounds = new Rectangle(24, 62, 352, 22),
            ForeColor = p.TextSecondary,
        };

        _lblCurrent = new Label
        {
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Bounds = new Rectangle(24, 84, 352, 24),
            ForeColor = p.AccentStrong,
            AutoEllipsis = true,
            UseMnemonic = false, // filenames may contain '&'
        };

        var btnChoose = new Button
        {
            Text = "Choose .wav…",
            Bounds = new Rectangle(24, 124, 130, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.AccentTeal,
            ForeColor = Color.White,
        };
        btnChoose.FlatAppearance.BorderSize = 0;
        btnChoose.Click += (_, _) => ChooseFile();

        _btnPreview = new Button
        {
            Text = "Preview",
            Bounds = new Rectangle(162, 124, 104, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.ButtonSecondaryBack,
            ForeColor = p.ButtonSecondaryFore,
        };
        _btnPreview.FlatAppearance.BorderColor = p.ButtonBorder;
        _btnPreview.Click += (_, _) => ReminderSound.Play(SoundFilePath);

        _btnReset = new Button
        {
            Text = "Use default",
            Bounds = new Rectangle(274, 124, 102, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.ButtonSecondaryBack,
            ForeColor = p.ButtonSecondaryFore,
        };
        _btnReset.FlatAppearance.BorderColor = p.ButtonBorder;
        _btnReset.Click += (_, _) => { SoundFilePath = ""; RefreshCurrent(); };

        var btnClose = new Button
        {
            Text = "Done",
            Bounds = new Rectangle(294, 182, 82, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.AccentTeal,
            ForeColor = Color.White,
            DialogResult = DialogResult.OK,
        };
        btnClose.FlatAppearance.BorderSize = 0;
        AcceptButton = btnClose;

        Controls.AddRange(new Control[]
        {
            _chkEnabled, lblLabel, _lblCurrent, btnChoose, _btnPreview, _btnReset, btnClose,
        });

        ResumeLayout(false);
        PerformLayout();

        RefreshCurrent();
        SyncEnabledState();
    }

    private void ChooseFile()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Choose a .wav sound",
            Filter = "WAV audio (*.wav)|*.wav",
            CheckFileExists = true,
        };
        if (!string.IsNullOrWhiteSpace(SoundFilePath))
        {
            try { dlg.InitialDirectory = Path.GetDirectoryName(SoundFilePath); } catch { }
        }
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        if (!ReminderSound.IsPlayable(dlg.FileName))
        {
            MessageBox.Show(this,
                "That file couldn't be loaded as a .wav sound. Please pick a standard PCM .wav file.",
                "Can't use that file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        SoundFilePath = dlg.FileName;
        RefreshCurrent();
        ReminderSound.Play(SoundFilePath); // let them hear the choice immediately
    }

    private void RefreshCurrent()
    {
        _lblCurrent.Text = string.IsNullOrWhiteSpace(SoundFilePath)
            ? "Default chime (built-in)"
            : Path.GetFileName(SoundFilePath);
        _btnReset.Enabled = !string.IsNullOrWhiteSpace(SoundFilePath);
    }

    private void SyncEnabledState()
    {
        bool on = _chkEnabled.Checked;
        _lblCurrent.Enabled = on;
        _btnPreview.Enabled = on;
    }
}
