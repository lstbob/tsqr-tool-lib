using System.Data;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
