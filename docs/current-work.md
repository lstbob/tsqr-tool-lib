# TSQR Tool Library — Current Work (DDD Analysis)

> **Purpose:** a snapshot of *what has been built so far* and how it is structured, so a
> developer can orient quickly. Companion to [`planned-work.md`](./planned-work.md), which
> proposes the path forward. Generated from a domain-driven analysis of the codebase on
> branch `chore/analysis` (= latest `main`, the "opencode" refactor).

## 1. What this is

A **community tool library** ("library of things") management system: members register tools
that other members can **borrow, reserve, return, and flag for repair**, with **identity-verified
membership**, **reservation queues**, **late-return fines**, and **per-location tool scarcity**.
The founding domain notes live in [`docs/tooldomain.txt`](./tooldomain.txt) and
[`docs/usersbreakdown.txt`](./usersbreakdown.txt).

The solution is **C# / .NET 9** built with **Domain-Driven Design + Clean Architecture** and
**MediatR** (CQRS-style commands).

## 2. Solution structure & maturity at a glance

| Project | Role | Maturity |
|---|---|---|
| `TSQR.ToolLibrary.Domain` | Aggregates, value objects, domain events, domain services, repository interfaces | 🟢 **Mostly implemented** |
| `TSQR.ToolLibrary.Common` | Base building blocks (`Entity<TId>`, `ValueObject`, `IAggregateRoot`) + validation extensions | 🟢 Implemented |
| `TSQR.ToolLibrary.Application` | MediatR command handlers (use cases) | 🟢 **Implemented** — commands + query handlers, domain events dispatched in-transaction |
| `TSQR.ToolLibrary.WebApi` | HTTP entry point | 🟢 **Implemented** — real controllers wired to Application, JWT auth, health endpoint |
| `TSQR.ToolLibrary.Infrastructure` | Persistence / external adapters | 🟢 **Implemented** — Dapper repositories, SqlRepository base, DapperUnitOfWork, TypeHandlers |
| `tests/.../Domain.UnitTests` | Unit tests | 🔴 **Removed** — directory exists, no source files |

**Dependency direction (Clean Architecture, correct):**
`WebApi → Application → Domain ← Common`. The Domain depends only on Common; there is no
outward dependency from Domain to Application/Infrastructure.

## 3. Ubiquitous language (current glossary)

| Term | Meaning in the model |
|---|---|
| **Tool** | A *catalog definition* of a tool (model, manufacturer, type, amortization rate, per-location scarcity). |
| **InventoryItem** | A *physical copy* of a Tool that actually circulates (has serial number, status, condition, holder). |
| **Member** | A person in the library; may be Regular, Repairman, LocationCoordinator, or Admin; must be verified to borrow. |
| **Loan** | A borrowing record binding a Member to an InventoryItem with checkout/due dates and accrued fine. |
| **Reservation** | A request to pick up an item on a future date; carries a queue position and pickup confirmation. |
| **MaintenanceRecord** | A repair ticket for an InventoryItem (Reported → InProgress → Completed). |
| **Policy** | Tool-type + location lending rules (max loan days, max renewals, late fee/day, max reservation days). |
| **Location / Country** | Where items live; basis for per-location scarcity and policy. |
| **ScarcityLevel / AmortizationRate / Condition** | Tracked attributes that classify availability, wear, and state. |

> **Note:** the codebase splits **Tool** (catalog) from **InventoryItem** (copy) — a good
> instinct that matches industry practice (see `planned-work.md` §"Record vs. Copy").

## 4. Domain model

### 4.1 Aggregates

Seven aggregate folders under `src/TSQR.ToolLibrary.Domain/Aggregates/`. Each root is
`Entity<TId>, IAggregateRoot`; IDs are value objects wrapping an `int`.

| Aggregate (root) | Key state | Behaviors | Domain events raised | Status |
|---|---|---|---|---|
| **Tool** | Model, Description, Manufacturer, Type, AmortizationRate, `ScarcityByLocation` | `Create`, `UpdateToolDetails`, `SetScarcityLevel`, `RemoveScarcityLevel` | `ToolRegisteredEvent` (raised by `Tool.Register()`), `InventoryItemRequiredEvent` (raised by `Tool.Register()` to defer InventoryItem creation) | 🟢 |
| **InventoryItem** | ToolId, OwnerId, SerialNumber, `Status`, `Condition`, holder, loan count, usage time, repair flag | `Loan`, `Return`, `Reserve`, `MarkAsLost`, `MarkForRepair`, `CompleteRepair` | `ItemLoanedDomainEvent`, `ToolReturnedEvent`, `ToolMarkedForRepairEvent` | 🟢 reservation state no longer duplicated on the item (see #33) |
| **Loan** | MemberId, ItemId, CheckoutDate, DueDate, `Status`, ReturnedDate, `FineAccrued` | `Create`, `EndLoan` (computes overdue + fine) | `LoanCreatedDomainEvent` (raised by `Loan.Create()`, side-effect on InventoryItem handled in `LoanCreatedDomainEventHandler`), `LoanOverdueDomainEvent` | 🟡 fine hard-coded; no renewal |
| **Reservation** | ItemId, MemberId, ReservationDate, ExpiryDate, `Status`, IsConfirmed, `QueuePosition` | `Create`, `ConfirmPickup`, `Activate`, `Cancel`, `Complete`, `MoveDownInQueue` | `ReservationCreatedDomainEvent` (raised by `Reservation.Create()`, side-effect on InventoryItem handled in `ReservationCreatedDomainEventHandler`), `ReservationConfirmedEvent`, `ReservationCancelledEvent` | 🟡 queue not automated |
| **Member** | Name, contact, `Status`, `IsVerified`, verifier/date, `Record` | `Create`, `Verify`, `Suspend`, `Ban`, `Reinstate`, `IsEligibleToBorrow` | `MemberVerifiedEvent` | 🟢 |
| **MaintenanceRecord** | ItemId, ReportedBy/Date, Description, `Status`, CompletedBy/Date, ResultingCondition | `Create`, `StartWork`, `Complete` | `ToolRepairedEvent` | 🟢 |
| **Policy** *(+ Location, Country, Manufacturer as supporting entities)* | ToolType, LocationId, late fee/day, max loan/renewal/reservation days | `Create`, `SetDetails` | — | 🟡 defined, not enforced anywhere |

Invariants are enforced in factories/methods via the validation extensions in Common
(e.g. non-empty strings, defined/non-default enums, dates not in past/future, positive ints).

### 4.2 Value objects, enums & state machines

- **Value objects:** all aggregate IDs (`ToolId`, `MemberId`, `InventoryItemId`, `LoanId`,
  `ReservationId`, `LocationId`, `CountryId`, `MaintenanceRecordId`, `PolicyId`,
  `ManufacturerId`) plus `Address`. They derive from a `ValueObject` base providing
  component-based equality. (Prevents primitive-obsession — good.)
- **Enums / state machines** (all use `NotSet = 0` as an invalid default, validated on use):
  - `ItemStatus`: Available → Reserved → Loaned → UnderMaintenance → Lost
  - `LoanStatus`: NotSet → Active → Returned / Overdue / Canceled
  - `ReservationStatus`: Pending → Confirmed → Active → Completed / Cancelled
  - `MaintenanceStatus`: Reported → InProgress → Completed
  - `MemberStatus`: Active / Suspended / Banned · `MembershipType`: Regular / Repairman / LocationCoordinator / Admin
  - `Condition`: New / Good / Fair / Repaired / Poor · `ScarcityLevel`: Low / Medium / High / Critical · `AmortizationRate`: Low / Medium / High · `ToolType`: Hand/Power/Gardening/Construction/Specialty/Other

### 4.3 Domain events

`IDomainEvent : INotification` (MediatR). Event records in `Domain/Events/`:

- **Raised and dispatched today:** `ToolRegisteredEvent`, `InventoryItemRequiredEvent`,
  `ItemLoanedDomainEvent`, `ToolReturnedEvent`, `LoanCreatedDomainEvent`,
  `ReservationCreatedDomainEvent`, `ReservationConfirmedEvent`, `ReservationCancelledEvent`,
  `ToolMarkedForRepairEvent`, `ToolRepairedEvent`, `LoanOverdueDomainEvent`,
  `MemberVerifiedEvent`.
- **Defined but never raised (stubs):** `PickupReminderEvent`, `ReturnReminderEvent`,
  `NextInLineNotificationEvent` — the notification side of the domain is not yet wired.

> ✅ Events are **collected from tracked aggregates and dispatched in-transaction** by
> `DomainEventOrchestrator` / `DomainEventDispatcher` (the eShop "Option A" pattern — dispatch
> before commit, clear on success). Handlers run inside the same `DapperUnitOfWork` transaction,
> so side effects on other aggregates (e.g. `InventoryItem.Loan()` triggered by `LoanCreatedDomainEvent`)
> commit atomically with the originating command. This was added in commit `9fed56a` and is wired
> in `Application/DependencyInjection.cs`.

### 4.4 Domain services

- **`FineService`** — overdue-fine calculation (days overdue × fee). Implemented, but the
  Application layer doesn't call it; `Loan.EndLoan` hard-codes `$1.00/day` instead.
- **`MemberVerificationService`** — eligibility + admin-can-verify checks (used by `VerifyMemberCommand`).
- **`ReservationQueueService`** — next-position, next-in-line, shift-after-cancel logic.
  Implemented but **not integrated** with the reservation commands.

### 4.5 Repository abstractions

`IRepository<TAggregateRoot, TId>` (GetById/Add/Update/Delete + `IUnitOfWork`) and
`IUnitOfWork` (SaveChangesAsync) are defined in the Domain. **Implementations live in
`TSQR.ToolLibrary.Infrastructure`** — a generic `SqlRepository<TEntity, TId>` paired with
`ISqlEntityMapping<TEntity>` (one mapping class per aggregate), a `DapperUnitOfWork` with
transient-failure retry (3 attempts, exponential backoff), and Dapper `TypeHandlers` for the
value-object IDs. Each bounded-context microservice uses the same stack.

## 5. Application layer (use cases)

Command handlers use the `IInteractor<TCommand, TResult>` pattern (MediatR was replaced).
Commands mutate a single aggregate per transaction and rely on domain events for side effects
on other aggregates (see §4.3). Read queries go through dedicated query interfaces
(`IDashboardQueries`, etc.) using raw Dapper SQL → DTO records (CQRS separation is clean —
no aggregate is loaded for a query).

| Area | Commands |
|---|---|
| Tool | `RegisterToolCommand` (emits `InventoryItemRequiredEvent`; InventoryItem created in handler) |
| Reservation | `ReserveToolCommand` (enforces ≤28-day advance; emits `ReservationCreatedDomainEvent`), `ConfirmPickupCommand`, `CancelReservationCommand` |
| Inventory | `ReturnToolCommand`, `MarkToolForRepairCommand`, `CompleteRepairCommand` |
| Loan | `LoanToolCommand` (emits `LoanCreatedDomainEvent`; `InventoryItem.Loan()` runs in handler) |
| Member | `VerifyMemberCommand` |

Handlers load aggregates via `IRepository<T>` and call `DomainEventOrchestrator.SaveEntitiesAsync`
(which dispatches events in-transaction and commits via `UnitOfWork.SaveChangesAsync`).
**Gaps:** queue automation (`MoveDownInQueue`/`ShiftQueueAfterCancellation`) not invoked;
reminder/next-in-line events still stubbed; `Policy` aggregate not enforced; `Loan.EndLoan` fine
still hard-coded.

## 6. WebApi, Infrastructure, Tests

- **WebApi:** real controllers wired to Application handlers, JWT Bearer auth, role-based
  authorization on commands, a `/api/health` endpoint, and Dockerfile/`docker/init.sql` for
  local Postgres bootstrapping.
- **Infrastructure:** `TSQR.ToolLibrary.Infrastructure` project — Dapper `SqlRepository<TEntity, TId>`
  base, per-aggregate `ISqlEntityMapping<T>` under `Dapper/Mappings/`, repository implementations
  under `Dapper/Repositories/`, `DapperUnitOfWork` + `DapperConnection`, and `TypeHandlers` for
  the value-object IDs. Referenced by the solution.
- **Tests:** `tests/unit-tests/TSQR.ToolLibrary.Domain.UnitTests/` exists but contains only
  build artifacts; the test sources were removed when `main` was aligned to the opencode refactor.

## 7. Known issues / gaps observed (verify before relying on)

These came out of the analysis and are worth confirming in code:

1. ~~**No persistence** — `IRepository`/`IUnitOfWork` have no implementations; nothing is stored.~~
   **Fixed** — Dapper-backed `SqlRepository` + `DapperUnitOfWork` exist in Infrastructure.
2. ~~**No API** — WebApi is a template shell, not connected to the Application layer.~~
   **Fixed** — real controllers, JWT auth, role-based authorization.
3. ~~**Domain events never published** — added to aggregates, but no dispatcher; the three reminder/
   next-in-line events are never even raised.~~
   **Fixed for the dispatch pipeline — `DomainEventOrchestrator`/`DomainEventDispatcher` publish
   events in-transaction (commit `9fed56a`); the reminder/next-in-line trio (`PickupReminderEvent`,
   `ReturnReminderEvent`, `NextInLineNotificationEvent`) are still defined-but-not-raised stubs.
4. **Reservation queue not automated** — `ReservationQueueService` and `MoveDownInQueue` exist but
   cancellation/return don't shift the queue or notify the next member.
5. **Fines not policy-driven** — `Loan.EndLoan` hard-codes `$1.00/day`; `Policy.LateFeePerDay`
   and `FineService` are unused.
6. **No loan renewal** — `Policy.MaxRenewalCount` exists but `Loan` has no `Renew()`.
7. **Policies unenforced** — `Policy` (max loan/reservation days) isn't consulted by any handler.
8. **No tests** — unit tests were removed; there is currently no safety net.
9. **Minor naming bugs** — `ManufacturerId` (typo), `MaxLoanRerservationDays` (typo); a possible
   `null` `CurrentHolderId` path around `ToolReturnedEvent` — confirm in code.

## 8. Bottom line for a developer

The **domain model is the mature part** and a solid foundation: aggregates, value objects,
events, and a clean dependency structure are in place, and the ubiquitous language matches the
problem space. The infrastructure to make it *run* is now in place — **persistence (Dapper-based
Infrastructure), a real API (WebApi with JWT auth), and in-transaction domain-event dispatch**.
What's still missing is the operational layer on top: **queue/notification automation, policy
enforcement, loan renewal, test coverage, and the still-stubbed reminder/next-in-line events**.
The recommended sequence for closing those gaps — informed by how real tool libraries and
existing platforms work — is in [`planned-work.md`](./planned-work.md).
