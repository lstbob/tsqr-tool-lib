using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

namespace TSQR.ToolLibrary.Infrastructure.UnitTests;

public sealed class MemberAccountRepositoryTests
{
    [Fact]
    public async Task GetCommunityIdBySubjectAsync_JoinsMemberAccountsToMembers()
    {
        var subject = Guid.NewGuid();
        var connection = new FakeSqlConnection { ScalarResult = 42 };
        var repository = new MemberAccountRepository(new FakeSqlUnitOfWork(connection));

        var communityId = await repository.GetCommunityIdBySubjectAsync(subject);

        Assert.Equal(42, communityId);
        Assert.Contains("FROM MemberAccounts ma", connection.LastSql);
        Assert.Contains("JOIN Members m ON m.Id = ma.MemberId", connection.LastSql);
        Assert.Contains("WHERE ma.AuthSubject = @AuthSubject", connection.LastSql);
        Assert.Equal(
            subject,
            connection.LastParameters!.GetType().GetProperty("AuthSubject")!.GetValue(connection.LastParameters));
    }

    [Fact]
    public async Task GetCommunityIdBySubjectAsync_ReturnsNullWhenSubjectIsUnlinked()
    {
        var connection = new FakeSqlConnection { ScalarResult = null };
        var repository = new MemberAccountRepository(new FakeSqlUnitOfWork(connection));

        var communityId = await repository.GetCommunityIdBySubjectAsync(Guid.NewGuid());

        Assert.Null(communityId);
    }

    private sealed class FakeSqlUnitOfWork(ISqlConnection connection) : ISqlUnitOfWork
    {
        public ISqlConnection Connection { get; } = connection;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    private sealed class FakeSqlConnection : ISqlConnection
    {
        public object? ScalarResult { get; init; }
        public string LastSql { get; private set; } = string.Empty;
        public object? LastParameters { get; private set; }

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null) =>
            Task.FromResult(Enumerable.Empty<T>());

        public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null) =>
            Task.FromResult(default(T));

        public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            LastSql = sql;
            LastParameters = parameters;
            return Task.FromResult((T)ScalarResult!);
        }

        public Task<int> ExecuteAsync(string sql, object? parameters = null) =>
            Task.FromResult(0);

        public IEnumerable<T> Query<T>(string sql, object? parameters = null) => [];

        public T? QuerySingleOrDefault<T>(string sql, object? parameters = null) => default;

        public T ExecuteScalar<T>(string sql, object? parameters = null) =>
            (T)ScalarResult!;

        public int Execute(string sql, object? parameters = null) => 0;

        public void Dispose() { }
    }
}
