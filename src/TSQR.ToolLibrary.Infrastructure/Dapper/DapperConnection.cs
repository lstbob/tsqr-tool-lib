using TSQR.ToolLibrary.Infrastructure.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Dapper;

internal sealed class DapperConnection : IDatabaseConnection
{
    private readonly SqlConnection _connection;
    private readonly IDbTransaction? _transaction;

    public DapperConnection(SqlConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        => SqlMapper.QueryAsync<T>(_connection, sql, parameters, _transaction);

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
        => await SqlMapper.QuerySingleOrDefaultAsync<T>(_connection, sql, parameters, _transaction);

    public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        => SqlMapper.ExecuteScalarAsync<T>(_connection, sql, parameters, _transaction)!;

    public Task<int> ExecuteAsync(string sql, object? parameters = null)
        => SqlMapper.ExecuteAsync(_connection, sql, parameters, _transaction);

    public IEnumerable<T> Query<T>(string sql, object? parameters = null)
        => SqlMapper.Query<T>(_connection, sql, parameters, _transaction);

    public T? QuerySingleOrDefault<T>(string sql, object? parameters = null)
        => SqlMapper.QuerySingleOrDefault<T>(_connection, sql, parameters, _transaction);

    public T ExecuteScalar<T>(string sql, object? parameters = null)
        => SqlMapper.ExecuteScalar<T>(_connection, sql, parameters, _transaction)!;

    public int Execute(string sql, object? parameters = null)
        => SqlMapper.Execute(_connection, sql, parameters, _transaction);

    public void Dispose()
    {
    }
}
