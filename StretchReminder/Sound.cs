using System.Media;
using System.Reflection;

namespace StretchReminder;

/// <summary>
/// Plays the reminder chime: a user-chosen .wav if one is set and readable,
/// otherwise the bundled default. Sound is optional, so every failure degrades
/// quietly (custom → default → system beep → silence) and never throws.
/// </summary>
public static class ReminderSound
{
    private static readonly byte[] _default = LoadDefault();

    private static byte[] LoadDefault()
    {
        try
        {
            using var s = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("StretchReminder.chime.wav");
            if (s == null) return Array.Empty<byte>();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
        catch { return Array.Empty<byte>(); }
    }

    /// <summary>True if <paramref name="path"/> is a .wav the player can load.</summary>
    public static bool IsPlayable(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        try
        {
            using var player = new SoundPlayer(path);
            player.Load(); // throws on a non-wav / unreadable file
            return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Play the reminder sound. Pass the user's chosen path (or null/empty for
    /// the default). Fire-and-forget: returns immediately.
    /// </summary>
    public static void Play(string? customPath)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
            {
                new SoundPlayer(customPath).Play();
                return;
            }
            PlayDefault();
        }
        catch
        {
            PlayDefault();
        }
    }

    private static void PlayDefault()
    {
        try
        {
            if (_default.Length > 0)
                new SoundPlayer(new MemoryStream(_default)).Play();
            else
                SystemSounds.Asterisk.Play();
        }
        catch
        {
            try { SystemSounds.Asterisk.Play(); } catch { /* give up silently */ }
        }
    }
}
