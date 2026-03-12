using ChartForge.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChartForge.Infrastructure.Services;

public class SqlQueryService : ISqlQueryService
{
    private readonly string ConnectionString;

    public SqlQueryService(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("SecondDefault")!;
    }
    public async Task<IEnumerable<IDictionary<string, object?>>> ExecuteQueryAsync(string sql)
    {
        // only accept SELECT Queries bish
        if (!sql.TrimStart().StartsWith("SELECT"))
        {
            throw new InvalidOperationException($"Only SELECT Queries are permitted");
        }

        await using SqlConnection conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        var result = await conn.QueryAsync(sql);

        var list = result
            .Select(row => (IDictionary<string, object?>)row)
            .ToList();

        return list;
        
    }
}