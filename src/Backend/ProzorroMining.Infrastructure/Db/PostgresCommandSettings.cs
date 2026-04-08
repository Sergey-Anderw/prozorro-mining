namespace ProzorroMining.Infrastructure.Db;

/// <summary>
/// Explicit SQL command settings used by Dapper operations.
/// </summary>
public sealed record PostgresCommandSettings(int CommandTimeoutSeconds);
