using System.Data;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

internal sealed class DapperConnection(IDbConnection connection, IDbTransaction? transaction)
    : ISqlConnection
{
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null) =>
        SqlMapper.QueryAsync<T>(connection, sql, parameters, transaction);

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null) =>
        await SqlMapper.QuerySingleOrDefaultAsync<T>(connection, sql, parameters, transaction);

    public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null) =>
        SqlMapper.ExecuteScalarAsync<T>(connection, sql, parameters, transaction)!;

    public Task<int> ExecuteAsync(string sql, object? parameters = null) =>
        SqlMapper.ExecuteAsync(connection, sql, parameters, transaction);

    public IEnumerable<T> Query<T>(string sql, object? parameters = null) =>
        SqlMapper.Query<T>(connection, sql, parameters, transaction);

    public T? QuerySingleOrDefault<T>(string sql, object? parameters = null) =>
        SqlMapper.QuerySingleOrDefault<T>(connection, sql, parameters, transaction);

    public T ExecuteScalar<T>(string sql, object? parameters = null) =>
        SqlMapper.ExecuteScalar<T>(connection, sql, parameters, transaction)!;

    public int Execute(string sql, object? parameters = null) =>
        SqlMapper.Execute(connection, sql, parameters, transaction);

    public void Dispose() { }
}
