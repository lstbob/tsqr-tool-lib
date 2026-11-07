# Document backend

Reserved for a future document/NoSQL backend (MongoDB, RavenDB, ...).

A document repository should implement `IDocumentRepository<TAggregateRoot>` from
`TSQR.ToolLibrary.Domain` and live under its own technology folder (for example
`Persistence/Document/MongoDb/`). Do not reuse the relational `ISqlEntityMapping`
or `ISqlUnitOfWork` contracts.
