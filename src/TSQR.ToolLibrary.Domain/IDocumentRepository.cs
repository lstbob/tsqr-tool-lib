namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Marker contract for document-oriented stores (MongoDB, RavenDB, ...).
/// Implementations can add backend-specific queries while relying on
/// <see cref="INoSqlRepository{TAggregateRoot}"/> for CRUD operations.
/// </summary>
public interface IDocumentRepository<TAggregateRoot> : INoSqlRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
}
