using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

namespace TSQR.ToolLibrary.Infrastructure.UnitTests;

public sealed class SqlRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ForwardsTypedIdToMapping()
    {
        var id = new StringId("item-1");
        var entity = new TestEntity(id);
        var mapping = new FakeMapping { Entity = entity };
        var repository = CreateRepository(mapping, new FakeSqlConnection());

        var result = await repository.GetByIdAsync(id);

        Assert.Same(entity, result);
        Assert.Same(id, mapping.LastRequestedId);
    }

    [Fact]
    public async Task AddAsync_UsesMappingToCreateTypedIdAndTracksAggregate()
    {
        var connection = new FakeSqlConnection { ScalarResult = "item-42" };
        var mapping = new FakeMapping { RowVersionResult = 17 };
        var collector = new FakeDomainEventCollector();
        var repository = CreateRepository(mapping, connection, collector);
        var entity = new TestEntity(new StringId("transient"));

        await repository.AddAsync(entity);

        Assert.Equal("item-42", entity.Id.Value);
        Assert.Equal(17, entity.RowVersion);
        Assert.Contains(entity, collector.TrackedAggregates);
        Assert.Contains(mapping.InsertSql + " RETURNING Id", connection.SqlStatements);
    }

    [Fact]
    public void Update_UsesMappingParametersRefreshesVersionAndTracksAggregate()
    {
        var connection = new FakeSqlConnection { RowVersionResult = 23 };
        var mapping = new FakeMapping();
        var collector = new FakeDomainEventCollector();
        var repository = CreateRepository(mapping, connection, collector);
        var entity = new TestEntity(new StringId("item-1"));

        repository.Update(entity);

        Assert.Equal(mapping.UpdateSql, connection.LastSql);
        Assert.Equal(23, entity.RowVersion);
        Assert.Same(entity, collector.TrackedAggregates.Single());
    }

    [Fact]
    public async Task Update_AfterAdd_SendsLoadedRowVersion()
    {
        var connection = new FakeSqlConnection { ScalarResult = "item-42" };
        var mapping = new FakeMapping { RowVersionResult = 17 };
        var repository = CreateRepository(mapping, connection);
        var entity = new TestEntity(new StringId("transient"));

        await repository.AddAsync(entity);
        repository.Update(entity);

        var parameters = Assert.IsType<UpdateParameters>(connection.LastParameters);
        Assert.Equal(17, parameters.RowVersion);
    }

    [Fact]
    public void Update_WhenRowVersionDoesNotMatch_ThrowsConcurrencyConflict()
    {
        var connection = new FakeSqlConnection { RowVersionResult = 0 };
        var repository = CreateRepository(new FakeMapping(), connection);
        var entity = new TestEntity(new StringId("item-1"));

        var exception = Assert.Throws<ConcurrencyConflictException>(
            () => repository.Update(entity));

        Assert.Equal("TestEntities", exception.EntityName);
    }

    [Fact]
    public void Delete_UsesMappingDeleteParameters()
    {
        var connection = new FakeSqlConnection();
        var mapping = new FakeMapping();
        var repository = CreateRepository(mapping, connection);
        var entity = new TestEntity(new StringId("item-1"));

        repository.Delete(entity);

        Assert.Equal(mapping.DeleteSql, connection.LastSql);
        var parameters = Assert.IsType<DeleteParameters>(connection.LastParameters);
        Assert.Equal("item-1", parameters.Id);
        Assert.Equal(0, parameters.RowVersion);
    }

    [Fact]
    public void Delete_WhenRowVersionDoesNotMatch_ThrowsConcurrencyConflict()
    {
        var connection = new FakeSqlConnection { ExecuteResult = 0 };
        var repository = CreateRepository(new FakeMapping(), connection);
        var entity = new TestEntity(new StringId("item-1"));

        var exception = Assert.Throws<ConcurrencyConflictException>(
            () => repository.Delete(entity));

        Assert.Equal("TestEntities", exception.EntityName);
    }

    private static SqlRepository<TestEntity, StringId> CreateRepository(
        FakeMapping mapping,
        FakeSqlConnection connection,
        FakeDomainEventCollector? collector = null) =>
        new(
            new FakeSqlUnitOfWork(connection),
            mapping,
            new FakeDialect(),
            collector ?? new FakeDomainEventCollector());

    private sealed class TestEntity(StringId id) : Entity<StringId>(id), IAggregateRoot
    {
    }

    private sealed class StringId(string value) : ValueObject
    {
        public string Value { get; } = value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
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
        public long RowVersionResult { get; init; } = 7;
        public int ExecuteResult { get; init; } = 1;
        public List<string> SqlStatements { get; } = [];
        public string? LastSql { get; private set; }
        public object? LastParameters { get; private set; }

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null) =>
            Task.FromResult(Enumerable.Empty<T>());

        public Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null) =>
            Task.FromResult(default(T));

        public Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            Record(sql, parameters);
            return Task.FromResult(GetScalar<T>());
        }

        public Task<int> ExecuteAsync(string sql, object? parameters = null) =>
            Task.FromResult(ExecuteResult);

        public IEnumerable<T> Query<T>(string sql, object? parameters = null) => [];

        public T? QuerySingleOrDefault<T>(string sql, object? parameters = null) => default;

        public T ExecuteScalar<T>(string sql, object? parameters = null)
        {
            Record(sql, parameters);
            return GetScalar<T>();
        }

        public int Execute(string sql, object? parameters = null)
        {
            Record(sql, parameters);
            return ExecuteResult;
        }

        private void Record(string sql, object? parameters)
        {
            LastSql = sql;
            LastParameters = parameters;
            SqlStatements.Add(sql);
        }

        private T GetScalar<T>() =>
            typeof(T) == typeof(long)
                ? (T)(object)RowVersionResult
                : (T)ScalarResult!;

        public void Dispose() { }
    }

    private sealed class FakeDialect : ISqlDialect
    {
        public string SelectInsertedIdSuffix => " RETURNING Id";

        public bool IsTransient(Exception ex) => false;
    }

    private sealed class FakeDomainEventCollector : IDomainEventCollector
    {
        private readonly List<Entity> _tracked = [];

        public IReadOnlyCollection<Entity> TrackedAggregates => _tracked;

        public void Track(Entity aggregate) => _tracked.Add(aggregate);

        public void Clear() => _tracked.Clear();
    }

    private sealed class FakeMapping : ISqlEntityMapping<TestEntity, StringId>
    {
        public TestEntity? Entity { get; init; }
        public StringId? LastRequestedId { get; private set; }
        public long RowVersionResult { get; init; } = 7;

        public string TableName => "TestEntities";
        public string InsertSql => "INSERT INTO TestEntities (Name) VALUES (@Name)";
        public string UpdateSql =>
            "UPDATE TestEntities SET Name = @Name WHERE Id = @Id AND xmin::text::bigint = @RowVersion RETURNING xmin::text::bigint";
        public string DeleteSql =>
            "DELETE FROM TestEntities WHERE Id = @Id AND xmin::text::bigint = @RowVersion";

        public Task<TestEntity?> GetByIdAsync(ISqlConnection db, StringId id)
        {
            LastRequestedId = id;
            return Task.FromResult(Entity);
        }

        public Task<long> GetRowVersionAsync(ISqlConnection db, StringId id) =>
            Task.FromResult(RowVersionResult);

        public object ToInsertParameters(TestEntity entity) => new InsertParameters(entity.Id.Value);

        public object ToUpdateParameters(TestEntity entity) =>
            new UpdateParameters(entity.Id.Value, entity.RowVersion);

        public object ToDeleteParameters(TestEntity entity) =>
            new DeleteParameters(entity.Id.Value, entity.RowVersion);

        public StringId CreateId(object databaseId) => new((string)databaseId);
    }

    private sealed record InsertParameters(string Id);

    private sealed record UpdateParameters(string Id, long RowVersion);

    private sealed record DeleteParameters(string Id, long RowVersion);
}
