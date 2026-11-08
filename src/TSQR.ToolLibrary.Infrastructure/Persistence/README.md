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

## Optimistic concurrency

Relational aggregates carry a `RowVersion` token on `Entity`. The PostgreSQL
implementation uses the `xmin` system column, so no schema column or migration
is required:

- mappings select `xmin::text::bigint AS RowVersion` and rehydrate the token;
- `UPDATE` and `DELETE` include `xmin::text::bigint = @RowVersion`;
- `UPDATE` returns the new token with `RETURNING xmin::text::bigint`;
- zero affected rows throw `ConcurrencyConflictException`, which the WebApi
  maps to HTTP 409.

`SqlRepository.AddAsync` reads the token after an insert so a newly created
entity can also be updated safely inside the same transaction.

## Adding a backend

1. Add `Persistence/<Technology>/` with its unit of work, mappings, and repositories.
2. Implement the appropriate Domain contract: `IRelationalRepository<TAggregateRoot, TId>` for relational stores or `INoSqlRepository<TAggregateRoot>` / `IDocumentRepository<TAggregateRoot>` for document stores.
3. Keep storage-specific types under the backend folder; do not leak SQL or document-store assumptions into the Domain contracts.
