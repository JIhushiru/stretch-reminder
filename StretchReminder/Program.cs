namespace StretchReminder;

static class Program
{
    [STAThread]
    static void Main()
    {
        // Must run before any UI (even the "already running" box) so DPI awareness applies.
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(true, "StretchReminder_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "Stretch Reminder is already running — look for the teal icon in the system tray.",
                "Stretch Reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Application.Run(new TrayAppContext());
    }
}
