using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace StretchReminder;

public enum ThemeKind { System, Light, Dark }

public sealed record Palette(
    bool Dark,
    Color WindowBack,
    Color FieldBack,
    Color TextPrimary,
    Color TextSecondary,
    Color TextMuted,
    Color AccentTeal,
    Color AccentStrong,
    Color AccentOrange,
    Color Caution,
    Color ButtonSecondaryBack,
    Color ButtonSecondaryFore,
    Color ButtonBorder,
    Color Danger)
{
    public static readonly Palette Light = new(
        Dark: false,
        WindowBack: Color.White,
        FieldBack: Color.White,
        TextPrimary: Color.FromArgb(30, 30, 30),
        TextSecondary: Color.FromArgb(90, 90, 90),
        TextMuted: Color.FromArgb(150, 150, 150),
        AccentTeal: Color.FromArgb(0, 137, 123),
        AccentStrong: Color.FromArgb(0, 105, 92),
        AccentOrange: Color.FromArgb(230, 81, 0),
        Caution: Color.FromArgb(198, 93, 0),
        ButtonSecondaryBack: Color.FromArgb(240, 240, 240),
        ButtonSecondaryFore: Color.FromArgb(40, 40, 40),
        ButtonBorder: Color.FromArgb(200, 200, 200),
        Danger: Color.FromArgb(190, 60, 60));

    public static readonly Palette DarkTheme = new(
        Dark: true,
        WindowBack: Color.FromArgb(32, 32, 32),
        FieldBack: Color.FromArgb(45, 45, 45),
        TextPrimary: Color.FromArgb(235, 235, 235),
        TextSecondary: Color.FromArgb(176, 176, 176),
        TextMuted: Color.FromArgb(128, 128, 128),
        AccentTeal: Color.FromArgb(38, 166, 154),
        AccentStrong: Color.FromArgb(77, 182, 172),
        AccentOrange: Color.FromArgb(255, 152, 0),
        Caution: Color.FromArgb(255, 183, 77),
        ButtonSecondaryBack: Color.FromArgb(55, 55, 55),
        ButtonSecondaryFore: Color.FromArgb(225, 225, 225),
        ButtonBorder: Color.FromArgb(85, 85, 85),
        Danger: Color.FromArgb(235, 120, 110));
}

/// <summary>
/// Resolves the active palette from the configured preference (System follows
/// the Windows app theme). Forms read <see cref="Current"/> at construction,
/// so a theme change rebuilds the main window and applies to future popups.
/// </summary>
public static class Theme
{
    public static Palette Current { get; private set; } = Palette.Light;

    public static void Refresh(ThemeKind preference) =>
        Current = IsDark(preference) ? Palette.DarkTheme : Palette.Light;

    public static bool IsDark(ThemeKind preference) => preference switch
    {
        ThemeKind.Dark => true,
        ThemeKind.Light => false,
        _ => SystemPrefersDark(),
    };

    private static bool SystemPrefersDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int light && light == 0;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    /// <summary>Dark title bar (DWMWA_USE_IMMERSIVE_DARK_MODE). Call once the handle exists.</summary>
    public static void ApplyTitleBar(Form form)
    {
        try
        {
            int dark = Current.Dark ? 1 : 0;
            _ = DwmSetWindowAttribute(form.Handle, 20, ref dark, sizeof(int));
        }
        catch
        {
            // older Windows builds without the attribute — cosmetic only
        }
    }
}
