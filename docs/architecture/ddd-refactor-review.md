# Architecture Review and DDD Refactor Proposal

## Scope and assumptions
- Scope reviewed: `FamilyBudgeting.API`, `FamilyBudgeting.Application`, `FamilyBudgeting.Domain`, `FamilyBudgeting.Infrastructure`.
- Assumption: this API is a modular monolith with PostgreSQL, Dapper-based writes/reads, and ASP.NET Core Identity.
- Goal: improve maintainability, explicitness, observability, scalability, and correctness for a fintech-grade workload.

## Executive assessment
The current solution already separates API, application service, domain model, and infrastructure projects. However, boundaries are blurred in ways that make invariants hard to enforce and increase coupling:

1. **Application/service layer carries domain orchestration complexity** (especially transactions), with many direct dependencies.
2. **Domain entities are anemic** (minimal invariant enforcement), so business correctness depends on service code paths.
3. **Read/write concerns are mixed across services and query services** without explicit module boundaries.
4. **Cross-cutting concerns (idempotency, optimistic concurrency, audit trail) are partial and not consistently modeled.**

Recommended direction: move to **DDD-style modular monolith** with explicit bounded contexts, aggregate roots, domain services for critical invariants, and a clear command/query split.

## Observed architecture signals

### 1) Composition root and registration are centralized but broad
- The API startup wires many repositories/query services/services in a single registration helper, indicating good discoverability but high global coupling.
- `ServiceInjectionHelper` registers all module components flatly, not by bounded context module.

### 2) Layer boundaries are partially inconsistent
- API controllers consume service interfaces from `Domain.Services.Interfaces`, while implementations reside in application project paths (`FamilyBudgeting.Application/Services`) but namespace `FamilyBudgeting.Domain.Services`.
- This namespace/project mismatch makes dependency direction and ownership harder to reason about.

### 3) Transaction use case is a God-service hotspot
- `TransactionService` has many dependencies (query services, repositories, other services, unit of work), indicating orchestration + business policy + integration all in one class.
- It performs multi-step operations (ledger access checks, category creation, budget category updates, account balance mutation) in one flow.
- This is functional but brittle for future change and hard to test at policy level.

### 4) Domain model is mostly data containers
- Entities such as `Ledger` and `Transaction` expose minimal behavior and little validation.
- `BaseEntity` provides soft-delete mechanics but no aggregate invariants, versioning, or explicit domain event hooks.

### 5) Data access has useful primitives but needs stronger contracts
- Dapper repositories and query services are clear and explicit.
- `QueryBuilder` supports locking (`FOR UPDATE` etc.), which is good for concurrency safety.
- However, lock semantics are invoked ad hoc in services rather than encoded in aggregate/application command patterns.

### 6) Fintech-grade concerns are only partially explicit
- There is transaction support and some lock usage.
- Missing explicit first-class constructs for:
  - idempotency keys for write commands,
  - optimistic concurrency tokens (row version / xmin mapping),
  - immutable audit trail for critical money movements,
  - outbox-style integration event publishing.

## Proposed DDD target architecture (recommended)

## 1) Bounded contexts (modular monolith)
Create clear modules with independent application/domain/infrastructure slices:
- **Identity & Access**: users, roles, authentication, authorization policies.
- **Ledger Management**: ledgers, memberships, permissions.
- **Accounting**: accounts, transaction posting, balances.
- **Budgeting**: budgets, budget categories, budget utilization.
- **Invitations/Collaboration**: invite lifecycle and acceptance.
- **Reporting**: dashboards/analytics (query-only read model module).

Each module should own:
- API endpoints (or endpoint group),
- application commands/queries + handlers,
- domain aggregates/value objects/domain services,
- infrastructure repositories/query adapters.

## 2) Tactical DDD model (focus on money movement)
### Aggregates
- **LedgerAggregate**: root for membership and high-level ledger invariants.
- **AccountAggregate**: balance and account-level constraints.
- **BudgetAggregate**: planning envelope and category allocations.
- **TransactionAggregate (or accounting journal entry)**: immutable posting record; avoid mutable financial facts when possible.

### Value objects
- `Money` (`MinorUnits`, `CurrencyId`) with arithmetic and sign rules.
- `DateRange` for query and validation clarity.
- `LedgerMemberRole`, `TransactionDirection`, `CategoryRef`.

### Domain services
- `PostingPolicyService` for transaction-to-balance/budget impact rules.
- `BudgetAllocationPolicy` for planned/current amount transitions.

## 3) Application layer refactor
Replace large service classes with **command handlers** and **query handlers**:
- `CreateTransactionCommandHandler`
- `UpdateTransactionCommandHandler`
- `TransferFundsCommandHandler`
- `ImportTransactionsCommandHandler`

Each handler should:
1. validate command shape,
2. load required aggregates with required lock strategy,
3. call aggregate/domain-service methods,
4. persist via repository + unit of work,
5. append audit record + outbox event,
6. return typed result.

This reduces constructor bloat and isolates behavior per use case.

## 4) Repository and query contracts
- Keep Dapper for performance.
- Split contracts by intent:
  - `I<Account>Repository` for aggregate persistence only,
  - query/read services for projections and list endpoints.
- Avoid command handlers calling other application services to prevent service mesh coupling; call domain services or repositories directly.

## 5) Concurrency and idempotency
For every write endpoint touching money/budgets:
- Require `Idempotency-Key` header.
- Store command fingerprint + response in `IdempotencyRecord` table.
- Enforce optimistic concurrency on aggregates (version/xmin check).
- Keep row-level locks only where invariants span multiple records in one transaction.

## 6) Observability and auditability
- Correlate logs by `CorrelationId`, `UserId`, `LedgerId`, `CommandName`.
- Emit structured domain events (`TransactionPosted`, `BudgetAdjusted`).
- Persist immutable `AuditLog` entries for critical operations:
  - actor,
  - before/after key values,
  - idempotency key,
  - timestamp (UTC),
  - source IP / client.

## 7) Error model and validation
- Standardize validation pipeline (FluentValidation or equivalent).
- Separate:
  - validation errors (400),
  - domain rule violations (409/422),
  - forbidden (403),
  - not found (404).
- Keep domain exceptions internal; map to API problem details consistently.

## Suggested staged migration plan

### Phase 0 (low risk, immediate)
1. Normalize namespaces to match project ownership (`Application.*`, `Domain.*`, etc.).
2. Introduce module folders by bounded context without behavior changes.
3. Add architecture tests for dependency direction.

### Phase 1 (transaction hotspot)
1. Extract `CreateTransaction` into command handler + domain policy service.
2. Introduce `Money` value object and enforce sign/currency invariants.
3. Add idempotency handling for transaction creation and transfer.
4. Add optimistic concurrency on account/budget updates.

### Phase 2 (aggregate hardening)
1. Move balance and budget mutation rules into aggregate methods.
2. Reduce direct DTO-to-entity reconstruction in services.
3. Add domain events + outbox table.

### Phase 3 (modularization)
1. Split registration per module (`AddAccountingModule`, `AddBudgetingModule`, etc.).
2. Isolate reporting queries into dedicated read model layer.
3. Add module-level integration tests and invariants tests.

## Key trade-offs considered

### Option A: full microservices now
- Pros: deployment isolation.
- Cons: significant consistency/operational complexity early.
- Verdict: not recommended now.

### Option B: modular monolith with DDD boundaries (recommended)
- Pros: strongest maintainability/correctness ratio at current scale; preserves transactional consistency.
- Cons: requires disciplined boundaries and architectural tests.
- Verdict: best fit.

### Option C: keep current structure, only cleanup
- Pros: least immediate work.
- Cons: ongoing complexity growth around transaction and budgeting rules.
- Verdict: only viable short-term.

## Concrete first refactor (recommended)
Start with `CreateTransaction` because it is highest leverage:
- Introduce `CreateTransactionCommand` + handler.
- Move category/budget/account mutation policy into `PostingPolicyService`.
- Replace ad hoc branching with explicit policy methods:
  - `ResolveCategory(...)`
  - `ApplyBudgetImpact(...)`
  - `ApplyAccountImpact(...)`
- Persist `TransactionPosted` event + audit record in same DB transaction.

Expected result:
- lower cognitive load,
- easier correctness proofs for money movement,
- better testability and rollback safety,
- cleaner path to broader DDD modularization.
