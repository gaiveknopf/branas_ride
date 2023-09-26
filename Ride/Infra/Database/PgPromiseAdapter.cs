using System.Data;
using Npgsql;
using Dapper;

namespace Ride.Infra.Database;

public class PgPromiseAdapter : IConnection
{
    private readonly IDbConnection _connection;

    public PgPromiseAdapter()
    {
        _connection = new NpgsqlConnection("Host=localhost;Port=5444;Pooling=true;Database=my-ride;User Id=postgres;Password=postgres;");
    }

    public async Task<IEnumerable<object>> Query(string statement, object data)
    {
        return await _connection.QueryAsync(statement, data);
    }

    public Task Close()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }

        return Task.CompletedTask;
    }
}
