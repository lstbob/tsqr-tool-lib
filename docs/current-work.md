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
| `TSQR.ToolLibrary.Application` | MediatR command handlers (use cases) | 🟡 **Partial** — commands only, no queries, events not dispatched |
| `TSQR.ToolLibrary.WebApi` | HTTP entry point | 🔴 **Stub** — default template (`WeatherForecast`), not wired to Application |
| `TSQR.ToolLibrary.Infrastructure` | Persistence / external adapters | 🔴 **Missing** — empty folder, not in the solution |
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
| **Tool** | Model, Description, Manufacturer, Type, AmortizationRate, `ScarcityByLocation` | `Create`, `UpdateToolDetails`, `SetScarcityLevel`, `RemoveScarcityLevel` | — (registration event raised by Application) | 🟢 |
| **InventoryItem** | ToolId, OwnerId, SerialNumber, `Status`, `Condition`, holder, loan count, usage time, repair flag | `Loan`, `Return`, `Reserve` (≤28d), `ClearReservation`, `MarkAsLost`, `MarkForRepair`, `CompleteRepair` | `ItemLoanedDomainEvent`, `ToolReturnedEvent` | 🟢 |
| **Loan** | MemberId, ItemId, CheckoutDate, DueDate, `Status`, ReturnedDate, `FineAccrued` | `Create`, `EndLoan` (computes overdue + fine) | `LoanOverdueDomainEvent` | 🟡 fine hard-coded; no renewal |
| **Reservation** | ItemId, MemberId, ReservationDate, ExpiryDate, `Status`, IsConfirmed, `QueuePosition` | `Create`, `ConfirmPickup`, `Activate`, `Cancel`, `Complete`, `MoveDownInQueue` | `ReservationConfirmedEvent`, `ReservationCancelledEvent` | 🟡 queue not automated |
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

`IDomainEvent : INotification` (MediatR). 14 event records in `Domain/Events/`:

- **Raised today:** `ToolRegisteredEvent`, `ItemLoanedDomainEvent`, `ToolReturnedEvent`,
  `ReservationConfirmedEvent`, `ReservationCancelledEvent`, `ToolMarkedForRepairEvent`,
  `ToolRepairedEvent`, `LoanOverdueDomainEvent`, `MemberVerifiedEvent`, `ToolReservedEvent`.
- **Defined but never raised (stubs):** `PickupReminderEvent`, `ReturnReminderEvent`,
  `NextInLineNotificationEvent` — the notification side of the domain is not yet wired.

> ⚠️ Events are **added to aggregates but never published** — there is no dispatcher in the
> Application/WebApi layer, so no handler ever runs (see §7).

### 4.4 Domain services

- **`FineService`** — overdue-fine calculation (days overdue × fee). Implemented, but the
  Application layer doesn't call it; `Loan.EndLoan` hard-codes `$1.00/day` instead.
- **`MemberVerificationService`** — eligibility + admin-can-verify checks (used by `VerifyMemberCommand`).
- **`ReservationQueueService`** — next-position, next-in-line, shift-after-cancel logic.
  Implemented but **not integrated** with the reservation commands.

### 4.5 Repository abstractions

`IRepository<TAggregateRoot, TId>` (GetById/Add/Update/Delete + `IUnitOfWork`) and
`IUnitOfWork` (SaveChangesAsync) are defined in the Domain. **No implementations exist** —
the Infrastructure project is empty.

## 5. Application layer (use cases)

MediatR command handlers, organized by sub-area. **8 commands, 0 queries:**

| Area | Commands |
|---|---|
| Tool | `RegisterToolCommand` |
| Reservation | `ReserveToolCommand` (enforces ≤28-day advance), `ConfirmPickupCommand`, `CancelReservationCommand` |
| Inventory | `ReturnToolCommand`, `MarkToolForRepairCommand`, `CompleteRepairCommand` |
| Member | `VerifyMemberCommand` |

Handlers load aggregates via `IRepository<T>` and call `UnitOfWork.SaveChangesAsync()`.
**Gaps:** no read/query side; queue automation (`MoveDownInQueue`/`ShiftQueueAfterCancellation`)
not invoked; domain events not dispatched.

## 6. WebApi, Infrastructure, Tests

- **WebApi:** `Program.cs` is the default .NET template (OpenAPI + sample `WeatherForecast`).
  No MediatR registration, no DI for repositories, no DbContext, **no real endpoints**.
- **Infrastructure:** placeholder folder, **0 source files, not referenced by the solution** —
  no EF Core, no repository implementations, no persistence at all.
- **Tests:** `tests/unit-tests/TSQR.ToolLibrary.Domain.UnitTests/` exists but contains only
  build artifacts; the test sources were removed when `main` was aligned to the opencode refactor.

## 7. Known issues / gaps observed (verify before relying on)

These came out of the analysis and are worth confirming in code:

1. **No persistence** — `IRepository`/`IUnitOfWork` have no implementations; nothing is stored.
2. **No API** — WebApi is a template shell, not connected to the Application layer.
3. **Domain events never published** — added to aggregates, but no dispatcher; the three reminder/
   next-in-line events are never even raised.
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
problem space. What's missing is everything that makes it *run*: **persistence (Infrastructure),
a real API (WebApi), event dispatching, queue/notification automation, policy enforcement, and
tests**. The recommended sequence for closing those gaps — informed by how real tool libraries
and existing platforms work — is in [`planned-work.md`](./planned-work.md).
