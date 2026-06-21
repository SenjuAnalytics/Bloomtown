namespace Bloomtown.Client.Console;

internal readonly record struct CommandDefinition(
    string Name,
    string[] Aliases,
    CommandCategory Category,
    string Description)
{
    public IEnumerable<string> AllNames
    {
        get
        {
            yield return Name;
            foreach (var alias in Aliases)
                yield return alias;
        }
    }
}