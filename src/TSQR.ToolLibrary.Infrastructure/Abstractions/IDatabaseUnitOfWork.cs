using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.Infrastructure.Abstractions;

public interface IDatabaseUnitOfWork : IUnitOfWork
{
    IDatabaseConnection Connection { get; }
}
