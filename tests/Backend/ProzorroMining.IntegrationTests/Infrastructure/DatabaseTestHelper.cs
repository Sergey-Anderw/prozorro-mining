using Dapper;
using Npgsql;

namespace ProzorroMining.IntegrationTests.Infrastructure;

internal static class DatabaseTestHelper
{
    public static async Task ExecuteAsync(string connectionString, string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, parameters);
    }

    public static async Task<T> QuerySingleAsync<T>(string connectionString, string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QuerySingleAsync<T>(sql, parameters);
    }

    public static async Task<IReadOnlyList<T>> QueryAsync<T>(string connectionString, string sql, object? parameters = null)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return (await connection.QueryAsync<T>(sql, parameters)).AsList();
    }
}
