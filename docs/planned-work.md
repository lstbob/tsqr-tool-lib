# TSQR Tool Library — Planned Work (Roadmap & Suggestions)

> **Purpose:** a researched path forward. Read [`current-work.md`](./current-work.md) first for
> what exists today. This document maps the existing model against how real community tool
> libraries and the established platforms work, then proposes a **prioritized roadmap** a solo
> developer can follow. Suggestions are grounded in prior art (sources at the end).

## 1. How the domain works in the real world (brief)

Community tool libraries / "libraries of things" are **membership-based lending** (mostly free
to borrow), not paid rental. The operational spine, consistent across operators and platforms:

- **Membership** gated on eligibility (residency/age) with **document-backed identity verification**
  and a signed **borrower agreement / liability waiver**; accounts with unpaid fees are **blocked**.
- **Catalog** of items with **barcodes**, grouped by **item type/category** that *governs how they
  circulate* (loan period, limits, fees).
- **Lending**: short loan periods (commonly **7 days** for tools), **renewals** unless a hold exists,
  **per-member item limits**.
- **Reservations/holds** join a **queue** against an item, with a **pickup window**, **freeze/suspend**,
  and **next-in-line** promotion; date-specific **bookings** for high-demand equipment.
- **Returns** check condition; damaged items route to **repair/maintenance**.
- **Overdue** handling is often **fine-free or low-fine** and trust-based, with an **escalation ladder**
  (reminder → 30-day → 60-day → collections) and **replacement-cost** billing for lost/damaged items.
- **Notifications** (hold-ready, due-soon, overdue, next-in-line) are the operational backbone.

**Prior art to borrow from:**
- **Koha** (open-source ILS) — the canonical **circulation domain model**: the **record↔copy split**,
  **item types**, and **circulation rules** keyed on *(branch, patron category, item type)*. Best
  source for vocabulary and state machines.
- **Lend Engine** — the closest functional blueprint for a library-of-things: member **account
  balances/credit**, reservations, **repair/cleaning assignments**, Stripe payments, self-serve portal.
- **myTurn** — dominant tool-library SaaS: check-in/out, barcode scanning, reservations, reminders,
  multi-location with transfers, utilization analytics.
- **Libib** — lightweight cataloging (circulation is paid).

## 2. Where the current model already aligns (keep)

The existing design is on the right track and should be preserved:

- ✅ **Tool (catalog) vs. InventoryItem (copy)** split — matches Koha's record↔copy, the single most
  important modeling decision. Keep and lean into it (holds queue against the Tool; loans bind to a copy).
- ✅ **Aggregates, value-object IDs, domain events, Clean Architecture layering** — solid DDD foundation.
- ✅ **Member verification, scarcity-per-location, maintenance tickets, reservation queue position** —
  all map to real-world concepts.

## 3. Gaps vs. real-world practice (what to add)

| Domain area | Real-world expectation | In the code today | Action |
|---|---|---|---|
| **Circulation rules** | One policy object keyed on *(location, member category, tool type)* → loan period, limits, renewals, fine rate | `Policy` exists but unenforced; rules hard-coded | Make `Policy`/`CirculationRule` first-class and **consult it in handlers** |
| **Renewals** | Renew unless a hold exists; capped count | `MaxRenewalCount` defined, no `Renew()` | Add `Loan.Renew()` honoring policy + holds |
| **Per-member limits** | Max items out at once | not modeled | Enforce in checkout handler |
| **Fines** | Policy-driven, often fine-free; replacement cost on loss/damage | hard-coded `$1/day`; `FineService` unused | Drive fines from `Policy`; wire `FineService`; add replacement-cost charge |
| **Account ledger / blocking** | Balance/credit; block borrowing when fees owed | not modeled | Add a Member **account balance** + block check |
| **Holds queue & pickup window** | Queue, Waiting state, pickup window, freeze, next-in-line | position tracked, **not automated**; reminder/next-in-line events never raised | Automate queue shift + raise/handle notification events |
| **Notifications** | Hold-ready, due-soon, overdue, next-in-line; escalation ladder | events defined but **never published** | Add event dispatch + notification handlers |
| **Transfers / In-Transit** | Items move between branches for pickup | single-location only | Add transfer/In-Transit state (later) |
| **Identity/privacy** | Store verification *outcome*, not document images | verification flag only | Keep as-is; store consent record, minimize PII |

## 4. Architectural must-dos (the system doesn't run yet)

These block everything else and should come first (see `current-work.md` §7):

1. **Infrastructure layer** — create the `TSQR.ToolLibrary.Infrastructure` project (add to the solution),
   implement **EF Core** `DbContext`, `IRepository<,>`/`IUnitOfWork`, and value-object/enum mappings.
2. **Wire the WebApi** — register MediatR + repositories in DI, replace the `WeatherForecast` template
   with real endpoints (or Minimal API) calling the Application commands.
3. **Domain-event dispatch** — publish aggregates' events after `SaveChangesAsync` (e.g. MediatR
   `INotificationPublisher` over an interceptor/outbox), so handlers actually fire.
4. **Tests** — reintroduce a test project with unit tests for aggregate invariants and handler tests;
   this is the safety net for everything below.

## 5. Prioritized roadmap (by bounded context)

Grouped near-term → later. The guiding principle (from Koha): **get Catalog + Circulation-Rule
engine + Loan lifecycle right first; most else hangs off them.**

### Near-term — make it runnable & operable (MVP)
1. **Foundations:** Infrastructure + EF Core persistence; WebApi wired to Application; event dispatch; test project. *(§4)*
2. **Catalog/Inventory:** finish copy lifecycle + barcodes + categories; expose CRUD/query endpoints; add **read/query handlers** (none exist today).
3. **Membership/Identity:** registration with eligibility check, member category, verification outcome + borrower-agreement consent, account-block flag.
4. **Lending:** a **CirculationRule** engine keyed on *(location, member category, tool type)*; `CreateLoan`/checkout, checkin, **`Renew`**; per-member limit + block-on-unpaid-fees enforcement; loan lifecycle incl. Overdue.
5. **Billing/Fines (thin):** Member **account ledger**; policy-driven overdue accrual; replacement-cost charge on lost/damaged; manual mark-paid.
6. **Notifications (thin):** due-soon + overdue reminders via event handlers.

### Mid-term — richer circulation
7. **Reservations:** automate the **queue** (shift on cancel/return), **Waiting/pickup-window** state, **next-in-line** promotion + notification, and **freeze/suspend**; add date-specific **Booking** for high-demand items.
8. **Repair/Maintenance:** auto-route returned-as-damaged items to a maintenance ticket; `NotForLoan`/reserved-maintenance state; repair cost tracking.
9. **Payments (full):** Stripe for fines/deposits; donations posted to the same ledger.
10. **Notifications (full):** hold-ready + next-in-line; escalation ladder (30/60-day) with optional collections; configurable templates; multi-channel.

### Later — differentiation & scale
11. **Reporting/Metrics:** utilization per item/category, active members & growth, on-time-return rate, underused-asset report; **impact dashboard** (CO₂/$ saved) for funders.
12. **Trust/Reputation (differentiator):** reputation score from on-time-return / loss / damage history; trust-tiered limits for peer-to-peer lending — incumbents don't model this.
13. **Self-serve & ecosystem:** member self-checkout/reservation portal; donation/acquisition wish-list; multi-location transfers; role-based admin permissions; import/export; multilingual/accessibility.

## 6. Suggested immediate next steps (concrete, ordered)

1. Add the **Infrastructure** project to the solution; implement EF Core `DbContext` + one repository
   (start with `Tool`/`InventoryItem`) and `UnitOfWork`.
2. Register MediatR + repositories in **WebApi** DI; add a real `POST /tools` endpoint backed by the
   existing `RegisterToolCommand` — proves the stack end-to-end.
3. Add **domain-event publishing** after save; implement a first handler (e.g. log `ToolRegisteredEvent`).
4. Stand up the **test project**; cover `InventoryItem` and `Loan` invariants + the register-tool handler.
5. Introduce the **CirculationRule** concept and refactor `Loan.EndLoan` to use `Policy`/`FineService`
   instead of the hard-coded `$1/day`; add `Loan.Renew()`.
6. Fix the small naming bugs (`ManufcaturerId`, `MaxLoanRerservationDays`) while touching those files.

## 7. Sources

- Koha circulation manual — https://koha-community.org/manual/23.11/en/html/circulation.html
- Koha item types — https://bywatersolutions.com/education/item-types-in-koha
- Lend Engine (Library of Things) — https://www.lend-engine.com/software-for-library-of-things
- myTurn (lending libraries) — https://myturn.com/lending-libraries/
- Berkeley Tool Lending Library (borrowing rules, renewals, fines, eligibility) — https://www.berkeleypubliclibrary.org/locations/tool-lending-library/borrowing-tools
- Oakland Tool Lending Library (rules) — https://oaklandlibrary.org/otll/rules-and-regulations/
- LA County Library — Tools (fine-free, library-card tie-in) — https://lacountylibrary.org/tools/
- Holds / pickup window / freeze behavior — https://aclibrary.org/faq/holds/ , https://kcls.org/faq/holds/
- USDN tool-lending toolkit (fines/escalation) — https://sustainableconsumption.usdn.org/initiatives-list/tool-lending-libraries
- myTurn — starting a tool library (agreements, donations, roles) — https://myturn.com/resource/starting-a-tool-library/
- Tool utilization / impact study — https://digitalcommons.unl.edu/cgi/viewcontent.cgi?article=1125&context=envstudtheses
