# Persistence layout

Backend-neutral repository contracts live in the Domain project:

- `IRepository<TAggregateRoot, TId>` — CRUD contract with no unit-of-work or storage assumptions.
- `IRelationalRepository<TAggregateRoot, TId>` — relational contract that adds `IUnitOfWork`.
- `INoSqlRepository<TAggregateRoot>` / `IDocumentRepository<TAggregateRoot>` — document-store CRUD contract.

Infrastructure implementations are grouped by storage technology:

- `Relational/Abstractions/` — relational contracts (`ISqlConnection`, `ISqlEntityMapping`, `ISqlUnitOfWork`, `ISqlDialect`, `IDbConnectionFactory`).
- `Relational/Dapper/` — Dapper implementation (`SqlRepository`, mappings, repositories, outbox).
- `Relational/Postgres/` — PostgreSQL connection factory and dialect.
- `InMemory/` — in-memory repositories for tests and local use.
- `Document/` — reserved for future document/NoSQL backends (MongoDB, RavenDB, ...).

## Adding a backend

1. Add `Persistence/<Technology>/` with its unit of work, mappings, and repositories.
2. Implement the appropriate Domain contract: `IRelationalRepository<TAggregateRoot, TId>` for relational stores or `INoSqlRepository<TAggregateRoot>` / `IDocumentRepository<TAggregateRoot>` for document stores.
3. Keep storage-specific types under the backend folder; do not leak SQL or document-store assumptions into the Domain contracts.
