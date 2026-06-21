using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.Infrastructure.Dapper;

/// <summary>
/// Generic Dapper-backed repository base. Lives in <c>Infrastructure.Dapper</c>
/// because it is shaped by Dapper's <see cref="ISqlUnitOfWork"/> /
/// <see cref="ISqlEntityMapping{TEntity}"/> contracts. A NoSQL or document
/// adapter would implement <c>IRepository&lt;T,TId&gt;</c> directly with its
/// own base class - never by deriving from this one.
/// </summary>
public class SqlRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : ValueObject
{
    private readonly ISqlUnitOfWork _uow;
    private readonly ISqlEntityMapping<TEntity> _mapping;

    public SqlRepository(ISqlUnitOfWork uow, ISqlEntityMapping<TEntity> mapping)
    {
        _uow = uow;
        _mapping = mapping;
    }

    public IUnitOfWork UnitOfWork => _uow;
    protected ISqlConnection Database => _uow.Connection;

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _mapping.GetByIdAsync(_uow.Connection, id);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var db = _uow.Connection;
        var id = await db.ExecuteScalarAsync<int>(
            _mapping.InsertSql, _mapping.ToInsertParameters(entity));

        entity.SetAssignedId((TId)Activator.CreateInstance(typeof(TId), [id])!);
    }

    public virtual void Update(TEntity entity)
    {
        var db = _uow.Connection;
        db.Execute(_mapping.UpdateSql, _mapping.ToUpdateParameters(entity));
    }

    public virtual void Delete(TEntity entity)
    {
        var db = _uow.Connection;
        db.Execute(_mapping.DeleteSql, new { Id = entity.Id });
    }
}