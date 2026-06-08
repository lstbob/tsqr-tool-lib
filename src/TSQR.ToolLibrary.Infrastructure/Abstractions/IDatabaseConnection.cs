namespace TSQR.ToolLibrary.Infrastructure.Abstractions;

public interface IDatabaseConnection : IDisposable
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
