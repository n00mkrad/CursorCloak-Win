using CursorCloak;

internal class Program
{
    public static bool Quiet = false;
    public static bool ErrorQuiet = false;

    private static void Main(string[] args)
    {
        string arg = args.FirstOrDefault("");
        var additionalArgs = args.Skip(1).ToList();

        ErrorQuiet = additionalArgs.Contains("qq");
        Quiet = ErrorQuiet || additionalArgs.Contains("q");


        if (arg == "show")
        {
            SetCursorVisibility(true);
        }
        else if (arg == "hide")
        {
            SetCursorVisibility(false);
        }
        else
        {

            if (ShellChecks.StartedByDoubleClick())
            {
                Console.WriteLine("This application needs to be run from a terminal (e.g. cmd) or script.");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadLine();
                return;
            }

            Log("No valid argument passed. Please call with \"hide\" or \"show\".");
        }
    }

    static void SetCursorVisibility(bool visible)
    {
        try
        {
            if (visible)
            {
                WinCursor.Show();
            }
            else
            {
                WinCursor.Hide();
            }

            Log($"Cursor set to {(visible ? "visible" : "hidden")}.");
        }
        catch (Exception ex)
        {
            Log($"Error setting cursor visibility.\n{ex.Message}\n{ex.StackTrace}", true);
        }
    }

    public static void Log(string message, bool important = false)
    {
        if (!important && Quiet)
            return;

        if (important && ErrorQuiet)
            return;

        Console.WriteLine(message);
    }
}