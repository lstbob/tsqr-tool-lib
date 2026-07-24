using System.Data;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

public interface ISqlConnection : IDisposable
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null);
    Task<int> ExecuteAsync(string sql, object? parameters = null);

    IEnumerable<T> Query<T>(string sql, object? parameters = null);
    T? QuerySingleOrDefault<T>(string sql, object? parameters = null);
    T ExecuteScalar<T>(string sql, object? parameters = null);
    int Execute(string sql, object? parameters = null);
}
