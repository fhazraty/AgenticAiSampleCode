using Microsoft.Data.SqlClient;
using System.Data;

namespace AgenticAISample.Data;

public sealed class SqlService
{
    private readonly string _connectionString;

    public SqlService(AgentConfig config)
    {
        _connectionString = config.Sql.ConnectionString;
    }

    public async Task<DataTable> QueryAsync(string sql, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var dt = new DataTable();
        dt.Load(reader);
        return dt;
    }

    public async Task<int> ExecuteAsync(string sql, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        return await cmd.ExecuteNonQueryAsync(ct);
    }
}

