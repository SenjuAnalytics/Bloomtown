using System.Text;

namespace Bloomtown.Client.Console;

/// <summary>
/// Reads console input with Up/Down command history navigation.
/// </summary>
internal sealed class ConsoleLineReader
{
    private const int MaxHistory = 50;
    private const string Prompt = "> ";

    private readonly List<string> _history = new();
    private int _historyIndex;

    public string? ReadLine()
    {
        var buffer = new StringBuilder();
        _historyIndex = _history.Count;

        System.Console.Write(Prompt);

        while (true)
        {
            var keyInfo = System.Console.ReadKey(intercept: true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    System.Console.WriteLine();
                    var line = buffer.ToString();
                    if (!string.IsNullOrWhiteSpace(line))
                        AddToHistory(line.Trim());
                    return line;

                case ConsoleKey.Backspace:
                    if (buffer.Length > 0)
                    {
                        buffer.Length--;
                        System.Console.Write("\b \b");
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (_historyIndex > 0)
                    {
                        _historyIndex--;
                        ReplaceBuffer(buffer, _history[_historyIndex]);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (_historyIndex < _history.Count)
                    {
                        _historyIndex++;
                        var text = _historyIndex < _history.Count ? _history[_historyIndex] : string.Empty;
                        ReplaceBuffer(buffer, text);
                    }
                    break;

                case ConsoleKey.Escape:
                    ReplaceBuffer(buffer, string.Empty);
                    _historyIndex = _history.Count;
                    break;

                default:
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        buffer.Append(keyInfo.KeyChar);
                        System.Console.Write(keyInfo.KeyChar);
                    }
                    break;
            }
        }
    }

    private static void ReplaceBuffer(StringBuilder buffer, string text)
    {
        while (buffer.Length > 0)
        {
            buffer.Length--;
            System.Console.Write("\b \b");
        }

        buffer.Append(text);
        System.Console.Write(text);
    }

    private void AddToHistory(string command)
    {
        if (_history.Count > 0 && _history[^1] == command)
            return;

        _history.Add(command);

        if (_history.Count > MaxHistory)
            _history.RemoveAt(0);
    }
}