using System.Text.Json.Serialization;

namespace StretchReminder;

/// <param name="TimerSeconds">Suggested duration for the built-in countdown button.</param>
/// <param name="GentleVariant">Low-impact replacement shown when BMI ≥ 30 (joint protection).</param>
public record Exercise(
    string Name,
    string Target,
    string Dose,
    string[] Steps,
    int TimerSeconds,
    string? Caution = null,
    Exercise? GentleVariant = null);

public record Session(string Title, string Blurb, Exercise[] Exercises);

public enum SessionOutcome { Completed, Snoozed, Skipped }

/// <summary>Which rotation the popups draw from.</summary>
public enum ProgramKind { Stretch, WeightLoss, Mix }

public class AppConfig
{
    public int IntervalMinutes { get; set; } = 45;
    public int SnoozeMinutes { get; set; } = 5;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProgramKind Program { get; set; } = ProgramKind.Stretch;

    /// <summary>0 = not set.</summary>
    public double HeightCm { get; set; }

    /// <summary>0 = not set.</summary>
    public double WeightKg { get; set; }

    public bool WaterEnabled { get; set; } = true;
    public int WaterIntervalMinutes { get; set; } = 60;

    /// <summary>Play a chime when a stretch/workout popup appears.</summary>
    public bool SoundEnabled { get; set; } = true;

    /// <summary>Path to a custom .wav to play on reminders. Empty = bundled default chime.</summary>
    public string SoundFilePath { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ThemeKind Theme { get; set; } = ThemeKind.System;

    [JsonIgnore]
    public double? Bmi => HeightCm > 0 && WeightKg > 0
        ? WeightKg / Math.Pow(HeightCm / 100.0, 2)
        : null;

    /// <summary>BMI ≥ 30: workout popups swap in joint-friendly variants.</summary>
    [JsonIgnore]
    public bool LowImpact => Bmi >= 30;

    public static string BmiCategory(double bmi) =>
        bmi < 18.5 ? "underweight"
        : bmi < 25 ? "healthy range"
        : bmi < 30 ? "overweight"
        : "obese";

    public void Sanitize()
    {
        IntervalMinutes = Math.Clamp(IntervalMinutes, 5, 480);
        SnoozeMinutes = Math.Clamp(SnoozeMinutes, 1, 60);
        WaterIntervalMinutes = Math.Clamp(WaterIntervalMinutes, 15, 240);
        if (!Enum.IsDefined(Program)) Program = ProgramKind.Stretch;
        if (!Enum.IsDefined(Theme)) Theme = ThemeKind.System;
        if (HeightCm != 0) HeightCm = Math.Clamp(HeightCm, 100, 250);
        if (WeightKg != 0) WeightKg = Math.Clamp(WeightKg, 30, 300);
    }
}

public class AppState
{
    public int StretchIndex { get; set; }
    public int WorkoutIndex { get; set; }

    /// <summary>In Mix mode: whether the next session is a workout (they alternate).</summary>
    public bool MixNextIsWorkout { get; set; }
}
