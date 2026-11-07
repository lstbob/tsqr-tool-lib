using System.Data;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
