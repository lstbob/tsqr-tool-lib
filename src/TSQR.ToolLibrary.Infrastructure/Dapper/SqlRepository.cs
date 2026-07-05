namespace TSQR.ToolLibrary.Infrastructure.Dapper;

/// <summary>
/// Generic Dapper-backed repository base. Lives in <c>Infrastructure.Dapper</c>
/// because it is shaped by Dapper's <see cref="ISqlUnitOfWork"/> /
/// <see cref="ISqlEntityMapping{TEntity}"/> contracts. A NoSQL or document
/// adapter would implement <c>IRepository&lt;T,TId&gt;</c> directly with its
/// own base class - never by deriving from this one.
/// </summary>
public class SqlRepository<TEntity, TId>(ISqlUnitOfWork uow, ISqlEntityMapping<TEntity> mapping)
    : IRepository<TEntity, TId>
    where TEntity : Entity<TId>, IAggregateRoot
    where TId : ValueObject
{
    public IUnitOfWork UnitOfWork => uow;
    protected ISqlConnection Database => uow.Connection;

    public virtual async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    )
    {
        return await mapping.GetByIdAsync(uow.Connection, (int)((dynamic)id!).Value);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        var db = uow.Connection;
        var id = await db.ExecuteScalarAsync<int>(
            mapping.InsertSql,
            mapping.ToInsertParameters(entity)
        );

        entity.SetAssignedId((TId)Activator.CreateInstance(typeof(TId), [id])!);
    }

    public virtual void Update(TEntity entity)
    {
        var db = uow.Connection;
        db.Execute(mapping.UpdateSql, mapping.ToUpdateParameters(entity));
    }

    public virtual void Delete(TEntity entity)
    {
        var db = uow.Connection;
        db.Execute(mapping.DeleteSql, new { Id = entity.Id });
    }
}

