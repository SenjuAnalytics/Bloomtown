namespace Bloomtown.Client.Console;

/// <summary>
/// Standardized console feedback prefixes for testing clarity.
/// </summary>
internal static class ConsoleOutput
{
    public static void Info(string message) =>
        System.Console.WriteLine($"[Info] {message}");

    public static void Success(string message) =>
        System.Console.WriteLine($"[Success] {message}");

    public static void Error(string message) =>
        System.Console.WriteLine($"[Error] {message}");

    /// <summary>
    /// Prints a titled multi-line block (e.g. server query results).
    /// </summary>
    public static void WriteBlock(string title, string body)
    {
        Info($"--- {title} ---");
        foreach (var line in body.Split('\n', StringSplitOptions.None))
            System.Console.WriteLine($"  {line}");
    }
}