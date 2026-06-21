namespace Bloomtown.Server.Persistence;

public sealed class PersistenceOptions
{
    public string DatabasePath { get; init; } = "data/bloomtown.db";

    /// <summary>How often the server writes a full snapshot to SQLite.</summary>
    public TimeSpan AutoSaveInterval { get; init; } = TimeSpan.FromSeconds(30);
}