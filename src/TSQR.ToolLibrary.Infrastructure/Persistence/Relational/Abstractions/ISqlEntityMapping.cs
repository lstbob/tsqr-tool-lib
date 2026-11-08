namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

/// <summary>
/// Non-generic mapping metadata shared by every relational aggregate mapping.
/// </summary>
public interface ISqlEntityMapping
{
    string TableName { get; }

    string InsertSql { get; }

    /// <summary>
    /// Update statement with an optimistic-concurrency predicate. It must end
    /// with <c>RETURNING</c> the new row version so the repository can refresh
    /// the entity after a successful write.
    /// </summary>
    string UpdateSql { get; }

    /// <summary>Delete statement with the optimistic-concurrency predicate.</summary>
    string DeleteSql { get; }
}

/// <summary>
/// Relational mapping for an aggregate. The mapping owns the SQL and the
/// identifier conversion, so the generic repository never has to know how an
/// aggregate's strongly-typed id is stored.
/// </summary>
public interface ISqlEntityMapping<TEntity, TId> : ISqlEntityMapping
    where TEntity : Entity<TId>
    where TId : ValueObject
{
    Task<TEntity?> GetByIdAsync(ISqlConnection db, TId id);

    /// <summary>
    /// Returns the current optimistic-concurrency token for a persisted row.
    /// Used to initialise a newly inserted entity without re-reading all of
    /// its columns.
    /// </summary>
    Task<long> GetRowVersionAsync(ISqlConnection db, TId id);

    object ToInsertParameters(TEntity entity);

    object ToUpdateParameters(TEntity entity);

    object ToDeleteParameters(TEntity entity);

    TId CreateId(object databaseId);
}
