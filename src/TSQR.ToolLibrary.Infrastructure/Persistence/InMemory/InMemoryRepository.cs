using System.Collections.Concurrent;

namespace TSQR.ToolLibrary.Infrastructure.Persistence;

public class InMemoryRepository<TAggregateRoot, TId> : IRepository<TAggregateRoot, TId>
    where TAggregateRoot : class, IAggregateRoot
    where TId : ValueObject
{
    private readonly ConcurrentDictionary<string, TAggregateRoot> _store = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();

    public IUnitOfWork UnitOfWork => _unitOfWork;

    public Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var key = id.ToString() ?? string.Empty;
        _store.TryGetValue(key, out var entity);
        return Task.FromResult(entity);
    }

    public Task AddAsync(TAggregateRoot entity, CancellationToken cancellationToken = default)
    {
        var key = GetEntityId(entity).ToString() ?? string.Empty;
        _store.TryAdd(key, entity);
        return Task.CompletedTask;
    }

    public void Update(TAggregateRoot entity)
    {
        var key = GetEntityId(entity).ToString() ?? string.Empty;
        _store[key] = entity;
    }

    public void Delete(TAggregateRoot entity)
    {
        var key = GetEntityId(entity).ToString() ?? string.Empty;
        _store.TryRemove(key, out _);
    }

    protected IReadOnlyCollection<TAggregateRoot> GetAll()
    {
        return _store.Values.ToList().AsReadOnly();
    }

    private static ValueObject GetEntityId(TAggregateRoot entity)
    {
        var idProperty = typeof(TAggregateRoot).GetProperty("Id");
        return (ValueObject)idProperty!.GetValue(entity)!;
    }
}
