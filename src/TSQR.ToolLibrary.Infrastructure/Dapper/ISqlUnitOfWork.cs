namespace TSQR.ToolLibrary.Infrastructure.Dapper;

/// <summary>
/// SQL/Dapper-specific unit of work. Exposes the live SQL connection so Dapper
/// repositories can issue queries inside the current transaction. Application
/// code depends only on <c>IUnitOfWork</c> from the Domain layer; this
/// interface is a Dapper adapter concern.
/// </summary>
public interface ISqlUnitOfWork : IUnitOfWork
{
    ISqlConnection Connection { get; }
}

