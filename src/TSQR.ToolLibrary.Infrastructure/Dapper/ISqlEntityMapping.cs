namespace TSQR.ToolLibrary.Infrastructure.Dapper;

/// <summary>
/// SQL/Dapper-specific entity mapping. Generates the SQL strings and parameter
/// objects used by <see cref="SqlRepository{TEntity,TId}"/> and provides
/// typed read primitives. SQL-only concern; lives here, not in a generic
/// Abstractions layer.
/// </summary>
public interface ISqlEntityMapping<TEntity>
{
    string TableName { get; }

    string InsertSql { get; }
    string UpdateSql { get; }
    string DeleteSql { get; }

    Task<TEntity?> GetByIdAsync(ISqlConnection db, int id);
    object ToInsertParameters(TEntity entity);
    object ToUpdateParameters(TEntity entity);
}