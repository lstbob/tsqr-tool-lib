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
        var mapping = new FakeMapping();
        var collector = new FakeDomainEventCollector();
        var repository = CreateRepository(mapping, connection, collector);
        var entity = new TestEntity(new StringId("transient"));

        await repository.AddAsync(entity);

        Assert.Equal("item-42", entity.Id.Value);
        Assert.Contains(entity, collector.TrackedAggregates);
        Assert.Equal(mapping.InsertSql + " RETURNING Id", connection.LastSql);
    }

    [Fact]
    public void Update_UsesMappingParametersAndTracksAggregate()
    {
        var connection = new FakeSqlConnection();
        var mapping = new FakeMapping();
        var collector = new FakeDomainEventCollector();
        var repository = CreateRepository(mapping, connection, collector);
        var entity = new TestEntity(new StringId("item-1"));

        repository.Update(entity);

        Assert.Equal(mapping.UpdateSql, connection.LastSql);
        Assert.Same(entity, collector.TrackedAggregates.Single());
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
        public string? LastSql { get; private set; }
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

        public T ExecuteScalar<T>(string sql, object? parameters = null)
        {
            LastSql = sql;
            LastParameters = parameters;
            return (T)ScalarResult!;
        }

        public int Execute(string sql, object? parameters = null)
        {
            LastSql = sql;
            LastParameters = parameters;
            return 1;
        }

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

        public string TableName => "TestEntities";
        public string InsertSql => "INSERT INTO TestEntities (Name) VALUES (@Name)";
        public string UpdateSql => "UPDATE TestEntities SET Name = @Name WHERE Id = @Id";
        public string DeleteSql => "DELETE FROM TestEntities WHERE Id = @Id";

        public Task<TestEntity?> GetByIdAsync(ISqlConnection db, StringId id)
        {
            LastRequestedId = id;
            return Task.FromResult(Entity);
        }

        public object ToInsertParameters(TestEntity entity) => new InsertParameters(entity.Id.Value);

        public object ToUpdateParameters(TestEntity entity) => new UpdateParameters(entity.Id.Value);

        public object ToDeleteParameters(TestEntity entity) => new DeleteParameters(entity.Id.Value);

        public StringId CreateId(object databaseId) => new((string)databaseId);
    }

    private sealed record InsertParameters(string Id);

    private sealed record UpdateParameters(string Id);

    private sealed record DeleteParameters(string Id);
}
