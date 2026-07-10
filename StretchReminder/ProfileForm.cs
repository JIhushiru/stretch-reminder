namespace StretchReminder;

/// <summary>
/// Small dialog for height/weight. Shows BMI live and explains how it adjusts
/// the workout program. Update the weight as it changes over time.
/// </summary>
public class ProfileForm : Form
{
    public double HeightCm => (double)_numHeight.Value;
    public double WeightKg => (double)_numWeight.Value;

    private readonly NumericUpDown _numHeight;
    private readonly NumericUpDown _numWeight;
    private readonly Label _lblBmi;
    private readonly Label _lblInfo;

    public ProfileForm(double heightCm, double weightKg)
    {
        var p = Theme.Current;

        SuspendLayout();

        Text = "Height & weight";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(360, 258);
        BackColor = p.WindowBack;
        HandleCreated += (_, _) => Theme.ApplyTitleBar(this);

        var lblHeight = new Label
        {
            Text = "Height (cm):",
            Bounds = new Rectangle(24, 26, 120, 26),
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = p.TextSecondary,
        };
        _numHeight = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 250,
            Value = (decimal)(heightCm > 0 ? Math.Clamp(heightCm, 100, 250) : 170),
            Bounds = new Rectangle(156, 25, 120, 28),
            BackColor = p.FieldBack,
            ForeColor = p.TextPrimary,
        };

        var lblWeight = new Label
        {
            Text = "Weight (kg):",
            Bounds = new Rectangle(24, 64, 120, 26),
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = p.TextSecondary,
        };
        _numWeight = new NumericUpDown
        {
            Minimum = 30,
            Maximum = 300,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = (decimal)(weightKg > 0 ? Math.Clamp(weightKg, 30, 300) : 70),
            Bounds = new Rectangle(156, 63, 120, 28),
            BackColor = p.FieldBack,
            ForeColor = p.TextPrimary,
        };

        _lblBmi = new Label
        {
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            Bounds = new Rectangle(24, 104, 312, 26),
        };

        _lblInfo = new Label
        {
            Font = new Font("Segoe UI", 9F),
            ForeColor = p.TextSecondary,
            Bounds = new Rectangle(24, 132, 312, 66),
        };

        var btnSave = new Button
        {
            Text = "Save",
            Bounds = new Rectangle(168, 210, 82, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.AccentTeal,
            ForeColor = Color.White,
            DialogResult = DialogResult.OK,
        };
        btnSave.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Cancel",
            Bounds = new Rectangle(258, 210, 82, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = p.WindowBack,
            ForeColor = p.TextSecondary,
            DialogResult = DialogResult.Cancel,
        };
        btnCancel.FlatAppearance.BorderColor = p.ButtonBorder;

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        Controls.AddRange(new Control[]
        {
            lblHeight, _numHeight, lblWeight, _numWeight, _lblBmi, _lblInfo, btnSave, btnCancel,
        });

        ResumeLayout(false);
        PerformLayout();

        _numHeight.ValueChanged += (_, _) => UpdateBmi();
        _numWeight.ValueChanged += (_, _) => UpdateBmi();
        UpdateBmi();
    }

    private void UpdateBmi()
    {
        double h = (double)_numHeight.Value / 100.0;
        double bmi = (double)_numWeight.Value / (h * h);
        string category = AppConfig.BmiCategory(bmi);
        _lblBmi.Text = $"BMI: {bmi:0.0} — {category}";

        var p = Theme.Current;
        if (bmi < 18.5)
        {
            _lblBmi.ForeColor = p.Caution;
            _lblInfo.Text = "Underweight — losing more weight isn't advisable; talk to a doctor " +
                            "before cutting calories. The exercises themselves are fine — just " +
                            "don't pair them with a calorie deficit.";
        }
        else if (bmi >= 30)
        {
            _lblBmi.ForeColor = p.AccentStrong;
            _lblInfo.Text = "Workout popups will use low-impact versions (step jacks, fast march, " +
                            "incline climbers) to protect your knees while the weight comes down. " +
                            "Update your weight as it changes.";
        }
        else
        {
            _lblBmi.ForeColor = p.AccentStrong;
            _lblInfo.Text = "Workouts stay standard. Update your weight here as it changes — " +
                            "low-impact mode kicks in automatically if BMI reaches 30.";
        }
    }
}
