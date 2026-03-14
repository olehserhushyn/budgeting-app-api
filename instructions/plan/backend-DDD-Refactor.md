# Architecture Review and DDD Refactor Proposal

## Scope and Assumptions

- **Scope reviewed**: `FamilyBudgeting.API`, `FamilyBudgeting.Application`, `FamilyBudgeting.Domain`, `FamilyBudgeting.Infrastructure`
- **Current state**: Modular monolith with PostgreSQL, Dapper-based data access, ASP.NET Core Identity
- **Goal**: Transform into a maintainable, explicit, observable, and correct fintech-grade system using DDD principles
- **Learning objectives**: Understand DDD tactical patterns, bounded contexts, CQRS, event sourcing basics, and message queuing

## Executive Assessment

### Current State Analysis

The solution demonstrates good separation of concerns with distinct API, Application, Domain, and Infrastructure projects. However, several architectural issues limit maintainability and correctness:

**Strengths:**

- Clean project structure with clear layer separation
- Dapper for performant data access
- Row-level locking support via `QueryBuilder` (FOR UPDATE)
- Unit of Work pattern for transaction management
- ASP.NET Core Identity integration

**Critical Issues:**

1. **Anemic Domain Model**
   - Entities are data containers with minimal behavior
   - `Transaction` entity has no validation or business rules
   - `Account.AddTransaction(int amount)` accepts raw amounts without sign validation
   - Business logic scattered across service layer

2. **God Service Anti-Pattern**
   - `TransactionService` has 17 dependencies (repositories, query services, other services)
   - 850+ lines handling orchestration, validation, domain logic, and integration
   - Methods like `CreateTransactionAsync` span 200+ lines with complex branching
   - Violates Single Responsibility Principle

3. **Namespace/Project Mismatch**
   - Services in `FamilyBudgeting.Application/Services/` use namespace `FamilyBudgeting.Domain.Services`
   - Interfaces in `FamilyBudgeting.Application/Services/Interfaces/`
   - Confuses dependency direction and ownership

4. **Missing Fintech-Grade Safeguards**
   - No idempotency keys for write operations
   - No optimistic concurrency (version tokens)
   - No immutable audit trail for money movements
   - No domain events or outbox pattern
   - Locking is ad-hoc, not encoded in aggregate patterns

5. **Implicit Domain Concepts**
   - Money represented as `int` (cents) without type safety
   - No `Money` value object with currency and arithmetic rules
   - Transaction direction logic scattered (expense = negative, income = positive)
   - Category creation mixed into transaction creation flow

6. **Read/Write Coupling**
   - Query services and repositories mixed in same service constructors
   - No clear CQRS separation
   - Reporting queries share same models as write operations

### Recommended Direction

Transform into a **DDD-style modular monolith** with:

- Explicit bounded contexts with clear boundaries
- Rich domain models with invariant enforcement
- Command/Query separation (CQRS lite)
- Domain events for cross-context communication
- Message queue integration for async workflows
- Proper value objects and aggregates

## Observed Architecture Signals

### 1) Composition Root and Registration

**Current State:**

- `ServiceInjectionHelper` registers all components flatly in three methods
- Good discoverability but high global coupling
- No module-based registration (e.g., `AddAccountingModule()`)

**Impact:** Makes it hard to understand module boundaries and dependencies

### 2) Layer Boundaries Are Inconsistent

**Current State:**

- Services physically in `FamilyBudgeting.Application/Services/`
- But use namespace `FamilyBudgeting.Domain.Services`
- Interfaces in `FamilyBudgeting.Application/Services/Interfaces/`
- Controllers reference `Domain.Services.Interfaces`

**Impact:** Confuses dependency direction, makes refactoring risky

### 3) TransactionService is a God Service

**Current State:**

- 17 constructor dependencies
- 850+ lines of code
- Handles: validation, orchestration, domain logic, integration, error handling
- `CreateTransactionAsync`: 200+ lines with nested conditionals

**Example Issues:**

```csharp
// Creates categories on-the-fly during transaction creation
if (categoryId == null && !string.IsNullOrWhiteSpace(request.BudgetCategoryTitle))
{
    // 50+ lines of category creation logic
}
```

**Impact:** Hard to test, maintain, and reason about

### 4) Domain Model is Anemic

**Current State:**

```csharp
// Transaction.cs - just a data container
public class Transaction : BaseEntity
{
    public Guid AccountId { get; private set; }
    public int Amount { get; private set; }
    // No validation, no business rules
}

// Account.cs - minimal behavior
public void AddTransaction(int amount)
{
    Balance += amount; // No validation of sign, currency, limits
    UpdatedAt = DateTime.UtcNow;
}
```

**Impact:** Business rules live in services, not domain

### 5) Data Access Has Good Primitives But Weak Contracts

**Current State:**

- `QueryBuilder` supports `ForUpdate()`, `ForNoKeyUpdate()`, `SkipLocked()`
- Locking invoked ad-hoc in services
- No aggregate-level concurrency strategy

**Example:**

```csharp
var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.AccountId)
    .ForUpdate()  // Lock applied manually
    .QueryFirstOrDefaultAsync();
```

**Impact:** Locking strategy not enforced, easy to forget

### 6) Missing Fintech-Grade Concerns

**Current State:**

- No idempotency keys
- No optimistic concurrency (version/xmin)
- No immutable audit trail
- No domain events
- No outbox pattern for reliable messaging

**Impact:** Risk of duplicate transactions, lost audit trail, data races

## Proposed DDD Target Architecture

### 1) Bounded Contexts (Modular Monolith)

Create clear modules with independent slices:

| Bounded Context       | Responsibilities                            | Key Aggregates         |
| --------------------- | ------------------------------------------- | ---------------------- |
| **Identity & Access** | Users, roles, authentication, authorization | User, Role             |
| **Ledger Management** | Ledgers, memberships, permissions           | Ledger, UserLedger     |
| **Accounting**        | Accounts, transactions, balances            | Account, Transaction   |
| **Budgeting**         | Budgets, categories, allocations            | Budget, BudgetCategory |
| **Collaboration**     | Invitations, acceptance workflow            | Invitation             |
| **Reporting**         | Dashboards, analytics (read-only)           | Read models            |

**Each module owns:**

- API endpoints (or endpoint group)
- Application commands/queries + handlers
- Domain aggregates/value objects/domain services
- Infrastructure repositories/query adapters

**Module Structure:**

```
FamilyBudgeting.Modules.Accounting/
├── Application/
│   ├── Commands/
│   │   ├── CreateTransaction/
│   │   │   ├── CreateTransactionCommand.cs
│   │   │   ├── CreateTransactionCommandHandler.cs
│   │   │   └── CreateTransactionCommandValidator.cs
│   │   └── TransferFunds/
│   └── Queries/
│       └── GetTransactions/
├── Domain/
│   ├── Aggregates/
│   │   ├── Account.cs
│   │   └── Transaction.cs
│   ├── ValueObjects/
│   │   ├── Money.cs
│   │   └── TransactionType.cs
│   ├── DomainEvents/
│   │   └── TransactionPosted.cs
│   └── Services/
│       └── PostingPolicyService.cs
└── Infrastructure/
    ├── Repositories/
    └── Queries/
```

### 2) Tactical DDD Model (Focus on Money Movement)

#### Aggregates

**AccountAggregate (Root: Account)**

```csharp
public class Account : AggregateRoot
{
    public AccountId Id { get; private set; }
    public UserId OwnerId { get; private set; }
    public Money Balance { get; private set; }
    public AccountType Type { get; private set; }
    private int _version; // Optimistic concurrency

    // Invariant: Balance cannot go below zero for non-credit accounts
    public Result PostTransaction(Money amount, TransactionType type)
    {
        var newBalance = type.IsDebit()
            ? Balance.Subtract(amount)
            : Balance.Add(amount);

        if (!Type.AllowsNegativeBalance && newBalance.IsNegative())
            return Result.Fail("Insufficient funds");

        Balance = newBalance;
        AddDomainEvent(new BalanceChanged(Id, Balance, amount));
        return Result.Success();
    }
}
```

**TransactionAggregate (Root: Transaction)**

```csharp
public class Transaction : AggregateRoot
{
    public TransactionId Id { get; private set; }
    public AccountId AccountId { get; private set; }
    public Money Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime PostedAt { get; private set; }

    // Immutable after creation - financial records don't change
    private Transaction() { }

    public static Result<Transaction> Create(
        AccountId accountId,
        Money amount,
        TransactionType type,
        DateTime postedAt)
    {
        // Validation
        if (amount.IsZero())
            return Result.Fail("Amount must be non-zero");

        var transaction = new Transaction
        {
            Id = TransactionId.New(),
            AccountId = accountId,
            Amount = amount,
            Type = type,
            PostedAt = postedAt
        };

        transaction.AddDomainEvent(new TransactionPosted(transaction));
        return Result.Success(transaction);
    }
}
```

**BudgetAggregate (Root: Budget)**

```csharp
public class Budget : AggregateRoot
{
    public BudgetId Id { get; private set; }
    public LedgerId LedgerId { get; private set; }
    public DateRange Period { get; private set; }
    private List<BudgetCategory> _categories = new();

    // Invariant: Total allocated cannot exceed budget total
    public Result AllocateToCategory(CategoryId categoryId, Money amount)
    {
        var category = _categories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (category == null)
        {
            category = new BudgetCategory(categoryId, amount);
            _categories.Add(category);
        }
        else
        {
            category.UpdateAllocation(amount);
        }

        AddDomainEvent(new BudgetCategoryAllocated(Id, categoryId, amount));
        return Result.Success();
    }

    public Result RecordSpending(CategoryId categoryId, Money amount)
    {
        var category = _categories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (category == null)
            return Result.Fail("Category not found in budget");

        var result = category.RecordSpending(amount);
        if (result.IsSuccess)
            AddDomainEvent(new BudgetSpendingRecorded(Id, categoryId, amount));

        return result;
    }
}
```

#### Value Objects

**Money**

```csharp
public class Money : ValueObject
{
    public int MinorUnits { get; private set; } // Cents
    public CurrencyId Currency { get; private set; }

    private Money(int minorUnits, CurrencyId currency)
    {
        MinorUnits = minorUnits;
        Currency = currency;
    }

    public static Money FromMinorUnits(int minorUnits, CurrencyId currency)
        => new Money(minorUnits, currency);

    public static Money FromMajorUnits(decimal amount, CurrencyId currency, int fractionalDigits)
        => new Money((int)(amount * Math.Pow(10, fractionalDigits)), currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(MinorUnits + other.MinorUnits, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");
        return new Money(MinorUnits - other.MinorUnits, Currency);
    }

    public bool IsNegative() => MinorUnits < 0;
    public bool IsZero() => MinorUnits == 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MinorUnits;
        yield return Currency;
    }
}
```

**TransactionType**

```csharp
public class TransactionType : ValueObject
{
    public static readonly TransactionType Income = new("Income", false);
    public static readonly TransactionType Expense = new("Expense", true);
    public static readonly TransactionType Transfer = new("Transfer", false);

    public string Name { get; private set; }
    public bool IsDebit { get; private set; }

    private TransactionType(string name, bool isDebit)
    {
        Name = name;
        IsDebit = isDebit;
    }

    public bool IsDebit() => IsDebit;
    public bool IsCredit() => !IsDebit;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
```

**DateRange**

```csharp
public class DateRange : ValueObject
{
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    public DateRange(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException("Start must be before end");
        Start = start;
        End = end;
    }

    public bool Contains(DateTime date) => date >= Start && date <= End;
    public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
```

#### Domain Services

**PostingPolicyService**

```csharp
public class PostingPolicyService
{
    public Result<PostingDecision> DeterminePosting(
        Account account,
        Money amount,
        TransactionType type,
        Budget? budget,
        CategoryId? categoryId)
    {
        // Validate account can accept transaction
        var accountResult = account.PostTransaction(amount, type);
        if (accountResult.IsFailure)
            return Result.Fail<PostingDecision>(accountResult.Error);

        // If budget specified, validate and record
        BudgetImpact? budgetImpact = null;
        if (budget != null && categoryId != null)
        {
            var budgetResult = budget.RecordSpending(categoryId, amount);
            if (budgetResult.IsFailure)
                return Result.Fail<PostingDecision>(budgetResult.Error);
            budgetImpact = new BudgetImpact(budget.Id, categoryId, amount);
        }

        return Result.Success(new PostingDecision(account.Id, amount, budgetImpact));
    }
}
```

### 3) Application Layer Refactor

Replace service classes with **CQRS-style handlers**:

#### Command Pattern

**Command Definition**

```csharp
public record CreateTransactionCommand(
    Guid UserId,
    Guid AccountId,
    decimal Amount,
    Guid TransactionTypeId,
    Guid? CategoryId,
    Guid? BudgetId,
    DateTime Date,
    string? Note,
    string IdempotencyKey) : ICommand<Guid>;
```

**Command Handler**

```csharp
public class CreateTransactionCommandHandler
    : ICommandHandler<CreateTransactionCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly PostingPolicyService _postingPolicy;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public async Task<Result<Guid>> Handle(
        CreateTransactionCommand command,
        CancellationToken ct)
    {
        // 1. Check idempotency
        var existing = await _idempotencyService
            .GetResultAsync<Guid>(command.IdempotencyKey);
        if (existing != null)
            return Result.Success(existing);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 2. Load aggregates with locking
            var account = await _accountRepository
                .GetByIdAsync(command.AccountId, forUpdate: true);
            if (account == null)
                return Result.NotFound("Account not found");

            Budget? budget = null;
            if (command.BudgetId.HasValue)
            {
                budget = await _budgetRepository
                    .GetByIdAsync(command.BudgetId.Value, forUpdate: true);
            }

            // 3. Execute domain logic
            var money = Money.FromMajorUnits(
                command.Amount,
                account.Balance.Currency,
                2);

            var transactionType = TransactionType.FromId(command.TransactionTypeId);

            var postingResult = _postingPolicy.DeterminePosting(
                account,
                money,
                transactionType,
                budget,
                command.CategoryId);

            if (postingResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<Guid>(postingResult.Error);
            }

            // 4. Create transaction
            var transactionResult = Transaction.Create(
                account.Id,
                money,
                transactionType,
                command.Date);

            if (transactionResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<Guid>(transactionResult.Error);
            }

            var transaction = transactionResult.Value;

            // 5. Persist
            await _transactionRepository.AddAsync(transaction);
            await _accountRepository.UpdateAsync(account);
            if (budget != null)
                await _budgetRepository.UpdateAsync(budget);

            // 6. Store idempotency record
            await _idempotencyService.StoreResultAsync(
                command.IdempotencyKey,
                transaction.Id);

            await _unitOfWork.CommitTransactionAsync();

            // 7. Dispatch domain events
            await _eventDispatcher.DispatchAsync(transaction.DomainEvents);
            await _eventDispatcher.DispatchAsync(account.DomainEvents);

            return Result.Success(transaction.Id.Value);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

#### Query Pattern

**Query Definition**

```csharp
public record GetTransactionsQuery(
    Guid UserId,
    Guid? LedgerId,
    DateRange Period,
    int Page,
    int PageSize) : IQuery<PaginatedTransactionList>;
```

**Query Handler**

```csharp
public class GetTransactionsQueryHandler
    : IQueryHandler<GetTransactionsQuery, PaginatedTransactionList>
{
    private readonly ITransactionQueryService _queryService;
    private readonly IAccessService _accessService;

    public async Task<Result<PaginatedTransactionList>> Handle(
        GetTransactionsQuery query,
        CancellationToken ct)
    {
        // 1. Authorization
        if (query.LedgerId.HasValue)
        {
            var hasAccess = await _accessService
                .CheckLedgerAccessAsync(query.UserId, query.LedgerId.Value);
            if (!hasAccess)
                return Result.Forbidden();
        }

        // 2. Execute query (no locking, read-only)
        var result = await _queryService.GetTransactionsAsync(
            query.LedgerId,
            query.Period,
            query.Page,
            query.PageSize);

        return Result.Success(result);
    }
}
```

### 4) Repository and Query Contracts

**Write Side (Repositories)**

```csharp
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(AccountId id, bool forUpdate = false);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    // No Delete - use soft delete on aggregate
}
```

**Read Side (Query Services)**

```csharp
public interface ITransactionQueryService
{
    Task<PaginatedTransactionList> GetTransactionsAsync(
        Guid? ledgerId,
        DateRange period,
        int page,
        int pageSize);

    Task<TransactionSummary> GetSummaryAsync(
        Guid? ledgerId,
        DateRange period);
}
```

**Key Principles:**

- Repositories work with aggregates only
- Query services return DTOs/read models
- No query service calls in command handlers (except for lookups)
- Keep Dapper for performance on read side

### 5) Concurrency and Idempotency

#### Idempotency Service

```csharp
public interface IIdempotencyService
{
    Task<T?> GetResultAsync<T>(string idempotencyKey);
    Task StoreResultAsync<T>(string idempotencyKey, T result);
}

// Database table
CREATE TABLE idempotency_records (
    idempotency_key VARCHAR(255) PRIMARY KEY,
    result_type VARCHAR(255) NOT NULL,
    result_data JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL,
    expires_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_idempotency_expires ON idempotency_records(expires_at);
```

#### Optimistic Concurrency

**Add version to aggregates:**

```csharp
public abstract class AggregateRoot
{
    public int Version { get; private set; }

    public void IncrementVersion() => Version++;
}
```

**Repository implementation:**

```csharp
public async Task UpdateAsync(Account account)
{
    var sql = @"
        UPDATE accounts
        SET balance = @Balance,
            version = @NewVersion,
            updated_at = @UpdatedAt
        WHERE id = @Id AND version = @CurrentVersion";

    var affected = await _connection.ExecuteAsync(sql, new
    {
        account.Id,
        account.Balance.MinorUnits,
        NewVersion = account.Version + 1,
        CurrentVersion = account.Version,
        UpdatedAt = DateTime.UtcNow
    });

    if (affected == 0)
        throw new ConcurrencyException("Account was modified by another transaction");

    account.IncrementVersion();
}
```

#### Row-Level Locking Strategy

**Use FOR UPDATE only when:**

- Updating multiple related aggregates in one transaction
- Reading to validate invariants before write

**Example:**

```csharp
// Transfer between accounts - lock both
var sourceAccount = await _accountRepository.GetByIdAsync(sourceId, forUpdate: true);
var destAccount = await _accountRepository.GetByIdAsync(destId, forUpdate: true);
```

### 6) Observability and Auditability

#### Domain Events

```csharp
public record TransactionPosted(
    TransactionId TransactionId,
    AccountId AccountId,
    Money Amount,
    TransactionType Type,
    DateTime PostedAt) : IDomainEvent;

public record BalanceChanged(
    AccountId AccountId,
    Money NewBalance,
    Money ChangeAmount) : IDomainEvent;

public record BudgetSpendingRecorded(
    BudgetId BudgetId,
    CategoryId CategoryId,
    Money Amount) : IDomainEvent;
```

#### Outbox Pattern

**Outbox table:**

```sql
CREATE TABLE outbox_events (
    id UUID PRIMARY KEY,
    aggregate_type VARCHAR(255) NOT NULL,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(255) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP NOT NULL,
    processed_at TIMESTAMP NULL,
    retry_count INT DEFAULT 0
);

CREATE INDEX idx_outbox_unprocessed ON outbox_events(processed_at)
WHERE processed_at IS NULL;
```

**Event dispatcher:**

```csharp
public class OutboxEventDispatcher : IDomainEventDispatcher
{
    private readonly IOutboxRepository _outboxRepository;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            await _outboxRepository.AddAsync(new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = @event.GetType().Name,
                EventData = JsonSerializer.Serialize(@event),
                OccurredAt = DateTime.UtcNow
            });
        }
    }
}
```

**Background processor:**

```csharp
public class OutboxProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var events = await _outboxRepository.GetUnprocessedAsync(100);

            foreach (var @event in events)
            {
                try
                {
                    await _messageBus.PublishAsync(@event);
                    await _outboxRepository.MarkProcessedAsync(@event.Id);
                }
                catch (Exception ex)
                {
                    await _outboxRepository.IncrementRetryAsync(@event.Id);
                    _logger.LogError(ex, "Failed to process outbox event {EventId}", @event.Id);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

#### Audit Trail

```sql
CREATE TABLE audit_log (
    id UUID PRIMARY KEY,
    aggregate_type VARCHAR(255) NOT NULL,
    aggregate_id UUID NOT NULL,
    action VARCHAR(100) NOT NULL,
    user_id UUID NOT NULL,
    before_state JSONB NULL,
    after_state JSONB NULL,
    idempotency_key VARCHAR(255) NULL,
    correlation_id UUID NOT NULL,
    ip_address VARCHAR(45) NULL,
    occurred_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_audit_aggregate ON audit_log(aggregate_type, aggregate_id);
CREATE INDEX idx_audit_user ON audit_log(user_id);
CREATE INDEX idx_audit_occurred ON audit_log(occurred_at);
```

**Audit interceptor:**

```csharp
public class AuditInterceptor : ICommandHandlerDecorator
{
    public async Task<Result<TResponse>> Handle<TCommand, TResponse>(
        TCommand command,
        Func<Task<Result<TResponse>>> next)
    {
        var beforeState = await CaptureStateAsync(command);
        var result = await next();
        var afterState = await CaptureStateAsync(command);

        await _auditRepository.AddAsync(new AuditEntry
        {
            AggregateType = typeof(TCommand).Name,
            Action = command.GetType().Name,
            UserId = _currentUser.Id,
            BeforeState = beforeState,
            AfterState = afterState,
            CorrelationId = _correlationContext.CorrelationId,
            OccurredAt = DateTime.UtcNow
        });

        return result;
    }
}
```

### 7) Error Model and Validation

#### Validation Pipeline (FluentValidation)

```csharp
public class CreateTransactionCommandValidator
    : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be positive");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account is required");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Idempotency key is required");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Transaction date cannot be in the future");
    }
}
```

#### Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    public ResultStatus Status { get; }

    public static Result<T> Success(T value)
        => new Result<T>(true, value, null, ResultStatus.Ok);

    public static Result<T> Fail(string error)
        => new Result<T>(false, default, error, ResultStatus.Error);

    public static Result<T> NotFound(string error = "Not found")
        => new Result<T>(false, default, error, ResultStatus.NotFound);

    public static Result<T> Forbidden(string error = "Forbidden")
        => new Result<T>(false, default, error, ResultStatus.Forbidden);
}

public enum ResultStatus
{
    Ok = 200,
    NotFound = 404,
    Forbidden = 403,
    Error = 500,
    ValidationError = 400,
    Conflict = 409
}
```

#### API Error Mapping

```csharp
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => new OkObjectResult(result.Value),
            ResultStatus.NotFound => new NotFoundObjectResult(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = result.Error
            }),
            ResultStatus.Forbidden => new ObjectResult(new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = result.Error
            }) { StatusCode = 403 },
            ResultStatus.ValidationError => new BadRequestObjectResult(new ProblemDetails
            {
                Status = 400,
                Title = "Validation Error",
                Detail = result.Error
            }),
            ResultStatus.Conflict => new ConflictObjectResult(new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = result.Error
            }),
            _ => new ObjectResult(new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = result.Error
            }) { StatusCode = 500 }
        };
    }
}
```

## Staged Migration Plan

### Phase 0: Foundation (Low Risk, 1-2 weeks)

**Goal:** Fix structural issues without changing behavior

**Tasks:**

1. **Normalize namespaces**
   - Move `FamilyBudgeting.Application/Services/*` to proper namespace
   - Update all using statements
   - Run all tests to verify no breakage

2. **Create module folders**

   ```
   FamilyBudgeting.Modules/
   ├── Accounting/
   ├── Budgeting/
   ├── LedgerManagement/
   ├── Identity/
   └── Reporting/
   ```

3. **Add architecture tests**

   ```csharp
   [Fact]
   public void Domain_Should_Not_Depend_On_Infrastructure()
   {
       var result = Types.InAssembly(typeof(Account).Assembly)
           .Should()
           .NotHaveDependencyOn("FamilyBudgeting.Infrastructure")
           .GetResult();

       Assert.True(result.IsSuccessful);
   }
   ```

4. **Document current architecture**
   - Create ADR-001: Current Architecture State
   - Map existing dependencies
   - Identify hotspots for refactoring

**Success Criteria:**

- All tests pass
- No behavior changes
- Clear module boundaries visible

### Phase 1: Transaction Hotspot (Medium Risk, 2-3 weeks)

**Goal:** Refactor CreateTransaction to command handler pattern

**Tasks:**

1. **Introduce Money value object**

   ```csharp
   // Start by wrapping existing int amounts
   public class Money
   {
       public int MinorUnits { get; }
       public CurrencyId Currency { get; }

       // Add conversion methods
       public static Money FromCents(int cents, CurrencyId currency)
           => new Money(cents, currency);
   }
   ```

2. **Extract PostingPolicyService**
   - Move transaction posting logic from TransactionService
   - Encapsulate sign adjustment logic
   - Handle category resolution

3. **Create CreateTransactionCommand + Handler**
   - Define command record
   - Implement handler with same logic as current service
   - Add FluentValidation validator
   - Wire up in DI container

4. **Add idempotency support**
   - Create IdempotencyRecord table
   - Implement IdempotencyService
   - Add middleware to extract Idempotency-Key header
   - Update handler to check/store idempotency

5. **Add optimistic concurrency**
   - Add Version column to accounts table
   - Update Account entity with version
   - Modify repository to check version on update

6. **Run side-by-side**
   - Keep old TransactionService.CreateTransactionAsync
   - Add new endpoint using command handler
   - Compare results in tests
   - Switch over when confident

**Success Criteria:**

- CreateTransaction via command handler works
- Idempotency prevents duplicates
- Optimistic concurrency catches conflicts
- All existing tests pass

### Phase 2: Aggregate Hardening (Medium Risk, 2-3 weeks)

**Goal:** Move business logic into domain entities

**Tasks:**

1. **Enrich Account aggregate**

   ```csharp
   public class Account : AggregateRoot
   {
       public Result PostTransaction(Money amount, TransactionType type)
       {
           // Validate invariants
           // Update balance
           // Emit domain event
       }
   }
   ```

2. **Enrich Budget aggregate**
   - Move budget category allocation logic
   - Add spending validation
   - Emit BudgetSpendingRecorded events

3. **Make Transaction immutable**
   - Remove setters
   - Use factory method for creation
   - Prevent updates (create compensating transaction instead)

4. **Add domain events**
   - Create event base class
   - Add events to aggregates
   - Implement in-memory event dispatcher

5. **Update command handlers**
   - Call aggregate methods instead of direct property manipulation
   - Remove business logic from handlers
   - Keep handlers thin (orchestration only)

**Success Criteria:**

- Business rules enforced in domain
- Domain events emitted
- Handlers are thin orchestrators
- Tests focus on domain behavior

### Phase 3: CQRS and Modularization (Low-Medium Risk, 3-4 weeks)

**Goal:** Complete separation of read/write, organize by modules

**Tasks:**

1. **Implement query handlers**
   - Create IQueryHandler<TQuery, TResponse> interface
   - Implement handlers for all read operations
   - Keep Dapper for query performance
   - Return DTOs optimized for UI needs

2. **Separate read models from write models**
   - Create dedicated DTOs for queries
   - Optimize queries for specific views
   - Consider denormalized read tables for complex reports

3. **Organize by bounded context modules**

   ```
   FamilyBudgeting.Modules.Accounting/
   ├── Application/
   │   ├── Commands/
   │   ├── Queries/
   │   └── DependencyInjection.cs  // AddAccountingModule()
   ├── Domain/
   └── Infrastructure/
   ```

4. **Module registration**

   ```csharp
   // Program.cs
   builder.Services.AddAccountingModule(builder.Configuration);
   builder.Services.AddBudgetingModule(builder.Configuration);
   builder.Services.AddLedgerManagementModule(builder.Configuration);
   ```

5. **Add module integration tests**
   - Test each module in isolation
   - Test cross-module communication via events
   - Verify module boundaries are respected

**Success Criteria:**

- Clear module boundaries
- Read/write separation complete
- Module-based registration
- Integration tests pass

### Phase 4: Event-Driven Integration (Medium Risk, 2-3 weeks)

**Goal:** Add message queue for async workflows and cross-module communication

**Tasks:**

1. **Implement outbox pattern**
   - Create outbox_events table
   - Implement OutboxEventDispatcher
   - Create background processor

2. **Add message bus abstraction**

   ```csharp
   public interface IMessageBus
   {
       Task PublishAsync<T>(T message) where T : IIntegrationEvent;
       Task SubscribeAsync<T>(Func<T, Task> handler) where T : IIntegrationEvent;
   }
   ```

3. **Choose message broker**
   - **Option A: RabbitMQ** (mature, feature-rich, operational overhead)
   - **Option B: Redis Streams** (simpler, already have Redis?, less features)
   - **Option C: PostgreSQL LISTEN/NOTIFY** (no new infrastructure, limited scalability)

   **Recommendation:** Start with PostgreSQL LISTEN/NOTIFY, migrate to RabbitMQ when needed

4. **Implement integration events**

   ```csharp
   public record TransactionPostedIntegrationEvent(
       Guid TransactionId,
       Guid AccountId,
       Guid LedgerId,
       decimal Amount,
       DateTime PostedAt) : IIntegrationEvent;
   ```

5. **Add async workflows**
   - Budget recalculation on transaction posted
   - Email notifications on budget threshold exceeded
   - Analytics aggregation

**Success Criteria:**

- Outbox pattern prevents message loss
- Async workflows execute reliably
- Modules communicate via events
- No direct module-to-module calls

### Phase 5: Advanced Patterns (Optional, 2-3 weeks)

**Goal:** Add advanced DDD patterns for complex scenarios

**Tasks:**

1. **Event Sourcing for audit-critical aggregates**
   - Consider event sourcing for Transaction aggregate
   - Immutable event log provides perfect audit trail
   - Can rebuild state from events

2. **Saga pattern for distributed transactions**
   - Transfer between accounts in different ledgers
   - Multi-step workflows with compensation

3. **Specification pattern for complex queries**

   ```csharp
   public class TransactionSpecification
   {
       public static ISpecification<Transaction> InDateRange(DateRange range)
           => new Specification<Transaction>(t => t.Date >= range.Start && t.Date <= range.End);
   }
   ```

4. **Repository caching strategy**
   - Add Redis for frequently accessed aggregates
   - Cache invalidation on updates

**Success Criteria:**

- Event sourcing provides complete audit trail
- Sagas handle complex workflows
- Performance improved with caching

## Key Trade-Offs and Decisions

### Decision Matrix

| Aspect            | Current State                       | Target State                       | Trade-Off                                                           |
| ----------------- | ----------------------------------- | ---------------------------------- | ------------------------------------------------------------------- |
| **Service Layer** | God services with many dependencies | Command/query handlers             | Gain: testability, clarity. Lose: simplicity for trivial operations |
| **Domain Model**  | Anemic entities                     | Rich aggregates with behavior      | Gain: correctness, encapsulation. Lose: some flexibility            |
| **Concurrency**   | Ad-hoc locking                      | Optimistic + selective pessimistic | Gain: scalability. Lose: must handle conflicts                      |
| **Read/Write**    | Mixed in services                   | CQRS separation                    | Gain: optimization, clarity. Lose: some duplication                 |
| **Modules**       | Flat registration                   | Bounded context modules            | Gain: maintainability. Lose: initial setup complexity               |
| **Events**        | None                                | Domain + integration events        | Gain: decoupling, auditability. Lose: eventual consistency          |

### Architecture Decision Records

#### ADR-001: Adopt Modular Monolith with DDD

**Status:** Proposed

**Context:**
Current architecture has good layer separation but suffers from:

- God services with high coupling
- Anemic domain model
- Missing fintech-grade safeguards (idempotency, audit trail)
- Unclear module boundaries

**Decision:**
Adopt DDD tactical patterns within a modular monolith:

- Organize by bounded contexts (Accounting, Budgeting, etc.)
- Rich domain models with aggregates and value objects
- CQRS for read/write separation
- Domain events for cross-module communication

**Consequences:**

_Easier:_

- Understanding module responsibilities
- Testing business logic in isolation
- Adding new features within clear boundaries
- Maintaining correctness of financial operations
- Auditing and compliance

_Harder:_

- Initial learning curve for team
- More files and structure
- Must maintain discipline around boundaries
- Some duplication between read/write models

**Alternative Considered:** Microservices

- Rejected because: team size, operational complexity, unclear boundaries
- Can evolve to microservices later if needed

---

#### ADR-002: Use Command/Query Handlers Instead of Services

**Status:** Proposed

**Context:**
Current services have 10-20 dependencies and handle multiple responsibilities.
`TransactionService` is 850+ lines with complex orchestration.

**Decision:**
Replace service classes with focused command and query handlers:

- One handler per use case
- Handlers orchestrate, domain models contain logic
- Use MediatR or similar for handler dispatch

**Consequences:**

_Easier:_

- Testing individual use cases
- Understanding what a use case does
- Adding new use cases without modifying existing code
- Applying cross-cutting concerns (validation, logging, idempotency)

_Harder:_

- More files to navigate
- Need to understand handler pattern
- Slightly more boilerplate

---

#### ADR-003: Implement Idempotency for All Write Operations

**Status:** Proposed

**Context:**
Financial operations must not be duplicated due to:

- Network retries
- User double-clicks
- Client-side bugs

Currently no idempotency protection exists.

**Decision:**
Require `Idempotency-Key` header for all write operations:

- Store key + result in database
- Return cached result if key seen before
- Keys expire after 24 hours

**Consequences:**

_Easier:_

- Preventing duplicate transactions
- Client retry logic
- Meeting financial compliance requirements

_Harder:_

- Clients must generate unique keys
- Additional database table and lookups
- Must handle key expiration

**Alternative Considered:** Database constraints

- Rejected because: doesn't cover all duplicate scenarios, harder to return original result

---

#### ADR-004: Use Optimistic Concurrency with Selective Pessimistic Locking

**Status:** Proposed

**Context:**
Need to prevent lost updates and race conditions on financial data.
Current approach uses pessimistic locking (FOR UPDATE) inconsistently.

**Decision:**

- Default: Optimistic concurrency with version numbers
- Use pessimistic locking (FOR UPDATE) only when:
  - Updating multiple related aggregates
  - High contention expected
  - Reading to validate invariants

**Consequences:**

_Easier:_

- Horizontal scalability (fewer locks held)
- Better performance for low-contention scenarios
- Clear when locks are needed

_Harder:_

- Must handle ConcurrencyException
- Clients may need to retry
- More complex than always locking

---

#### ADR-005: Outbox Pattern for Reliable Event Publishing

**Status:** Proposed

**Context:**
Need to publish domain events to message bus reliably.
Cannot lose events even if message bus is down.

**Decision:**
Implement outbox pattern:

- Write events to outbox table in same transaction as aggregate
- Background processor publishes events to message bus
- Mark as processed after successful publish

**Consequences:**

_Easier:_

- Guaranteed event delivery
- Transactional consistency
- Can replay events if needed

_Harder:_

- Additional table and background processor
- Events delivered at-least-once (must be idempotent)
- Slight delay in event processing

**Alternative Considered:** Direct publish to message bus

- Rejected because: can lose events if bus is down or transaction rolls back

---

#### ADR-006: PostgreSQL LISTEN/NOTIFY for Initial Message Bus

**Status:** Proposed

**Context:**
Need message bus for async workflows and cross-module communication.
Want to avoid operational complexity early.

**Decision:**
Start with PostgreSQL LISTEN/NOTIFY:

- No new infrastructure
- Good enough for single-instance deployment
- Can migrate to RabbitMQ later if needed

**Consequences:**

_Easier:_

- No new infrastructure to manage
- Simple to implement
- Leverages existing PostgreSQL

_Harder:_

- Limited to single PostgreSQL instance
- No message persistence if no listeners
- Must migrate if scaling beyond single instance

**Migration Path:** When needed, switch to RabbitMQ with same IMessageBus interface

## Learning Path and Resources

### DDD Fundamentals

1. **Book:** "Domain-Driven Design" by Eric Evans (blue book)
2. **Book:** "Implementing Domain-Driven Design" by Vaughn Vernon (red book)
3. **Video:** "Domain-Driven Design: The Good Parts" by Jimmy Bogard
4. **Practice:** Identify aggregates in current codebase

### Tactical Patterns

1. **Aggregates:** Study Account, Transaction, Budget as aggregate examples
2. **Value Objects:** Implement Money, DateRange, TransactionType
3. **Domain Events:** Add events to one aggregate, observe behavior
4. **Repositories:** Understand aggregate persistence patterns

### CQRS

1. **Article:** "CQRS" by Martin Fowler
2. **Video:** "CQRS and Event Sourcing" by Greg Young
3. **Practice:** Separate one read operation from write operation

### Event-Driven Architecture

1. **Pattern:** Outbox pattern for reliable messaging
2. **Tool:** RabbitMQ or PostgreSQL LISTEN/NOTIFY
3. **Practice:** Implement one async workflow (e.g., budget recalculation)

### Message Queuing

1. **Tool:** RabbitMQ tutorials (if chosen)
2. **Pattern:** Pub/Sub vs Request/Reply
3. **Practice:** Implement event publishing and subscription

## Success Metrics

### Code Quality

- Cyclomatic complexity < 10 per method
- Service classes < 200 lines
- Test coverage > 80% for domain logic

### Architecture

- Clear module boundaries (no cross-module dependencies)
- Domain logic in domain layer (not services)
- All write operations idempotent

### Operational

- Zero duplicate transactions
- Complete audit trail for all money movements
- < 100ms p95 latency for transaction creation

### Team

- Team can explain bounded contexts
- New features added without modifying existing code
- Bugs caught by domain model validation

## Next Steps

1. **Review this plan with team** - Get buy-in and feedback
2. **Start Phase 0** - Low-risk foundation work
3. **Create ADR documents** - Document decisions as you go
4. **Set up architecture tests** - Enforce boundaries automatically
5. **Pair program on Phase 1** - Share knowledge while refactoring
6. **Iterate and learn** - Adjust plan based on what you learn

## Questions to Consider

1. **What happens when a transaction fails halfway through?**
   - Rollback via Unit of Work
   - Idempotency prevents retry issues
   - Audit log shows attempt

2. **How do we handle currency conversion?**
   - Money value object enforces same currency
   - Conversion is explicit domain service
   - Exchange rates are separate aggregate

3. **What if budget and account are in different databases?**
   - Saga pattern for distributed transaction
   - Eventual consistency via events
   - Compensation logic for failures

4. **How do we migrate existing data?**
   - Phase 0: No data changes
   - Phase 1: Add version column with default value
   - Phase 2+: Gradual migration with dual-write

5. **What if the team finds this too complex?**
   - Start with Phase 0 and 1 only
   - Evaluate after 1 month
   - Adjust based on team feedback
   - Complexity should reduce over time as patterns become familiar

---

**Remember:** The goal is not perfect architecture, but maintainable architecture that the team can work with effectively. Start small, learn, and iterate.
