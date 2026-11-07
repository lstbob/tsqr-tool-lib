namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

/// <summary>
/// Relational mapping for an aggregate. The mapping owns the SQL and the
/// identifier conversion, so the generic repository never has to know how an
/// aggregate's strongly-typed id is stored.
/// </summary>
public interface ISqlEntityMapping<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : ValueObject
{
    string TableName { get; }

    string InsertSql { get; }
    string UpdateSql { get; }
    string DeleteSql { get; }

    Task<TEntity?> GetByIdAsync(ISqlConnection db, TId id);

    object ToInsertParameters(TEntity entity);

    object ToUpdateParameters(TEntity entity);

    object ToDeleteParameters(TEntity entity);

    TId CreateId(object databaseId);
}
