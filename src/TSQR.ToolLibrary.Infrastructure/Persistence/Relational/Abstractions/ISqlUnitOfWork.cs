using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

public interface ISqlUnitOfWork : IUnitOfWork
{
    ISqlConnection Connection { get; }
}
