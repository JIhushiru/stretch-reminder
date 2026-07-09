using System.Text.Json;

namespace StretchReminder;

/// <summary>Persists config and rotation state to %APPDATA%\StretchReminder.</summary>
public static class Storage
{
    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StretchReminder");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public static AppConfig LoadConfig()
    {
        var config = Load<AppConfig>("config.json") ?? new AppConfig();
        config.Sanitize();
        return config;
    }

    public static void SaveConfig(AppConfig config) => Save("config.json", config);

    public static AppState LoadState()
    {
        var state = Load<AppState>("state.json") ?? new AppState();
        if (state.StretchIndex < 0 || state.StretchIndex >= Exercises.StretchSessions.Length)
            state.StretchIndex = 0;
        if (state.WorkoutIndex < 0 || state.WorkoutIndex >= Exercises.WorkoutSessions.Length)
            state.WorkoutIndex = 0;
        return state;
    }

    public static void SaveState(AppState state) => Save("state.json", state);

    private static T? Load<T>(string name) where T : class
    {
        try
        {
            var path = Path.Combine(Dir, name);
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : null;
        }
        catch
        {
            return null; // corrupt or unreadable — fall back to defaults
        }
    }

    private static void Save<T>(string name, T value)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(Path.Combine(Dir, name), JsonSerializer.Serialize(value, JsonOpts));
        }
        catch
        {
            // persistence is best-effort; the app keeps working from memory
        }
    }
}
