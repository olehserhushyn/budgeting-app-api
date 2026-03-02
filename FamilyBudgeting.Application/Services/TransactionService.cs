using Ardalis.Result;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
// using FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories; // not used - budget category DTO is in Categories namespace

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionQueryService _transactionQueryService;
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ICategoryQueryService _categoryQueryService;
        private readonly IBudgetQueryService _budgetQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;

        private readonly ITransactionCreateHandler _transactionCreateHandler;
        private readonly ITransactionUpdateHandler _transactionUpdateHandler;
        private readonly ITransactionDeleteHandler _transactionDeleteHandler;
        private readonly ITransactionTransferHandler _transactionTransferHandler;
        private readonly ITransactionImportHandler _transactionImportHandler;

        public TransactionService(
            ITransactionQueryService transactionQueryService,
            IUserLedgerQueryService userLedgerQueryService,
            ILedgerQueryService ledgerQueryService,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ICategoryQueryService categoryQueryService,
            IBudgetQueryService budgetQueryService,
            IBudgetCategoryQueryService budgetCategoryQueryService,
            ITransactionCreateHandler transactionCreateHandler,
            ITransactionUpdateHandler transactionUpdateHandler,
            ITransactionDeleteHandler transactionDeleteHandler,
            ITransactionTransferHandler transactionTransferHandler,
            ITransactionImportHandler transactionImportHandler)
        {
            _transactionQueryService = transactionQueryService;
            _userLedgerQueryService = userLedgerQueryService;
            _ledgerQueryService = ledgerQueryService;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _categoryQueryService = categoryQueryService;
            _budgetQueryService = budgetQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _transactionCreateHandler = transactionCreateHandler;
            _transactionUpdateHandler = transactionUpdateHandler;
            _transactionDeleteHandler = transactionDeleteHandler;
            _transactionTransferHandler = transactionTransferHandler;
            _transactionImportHandler = transactionImportHandler;
        }

        public async Task<Result<Guid>> CreateTransactionAsync(Guid userId, CreateTransactionRequest request)
        {
            return await _transactionCreateHandler.HandleAsync(userId, request);
        }

        public async Task<Result<bool>> UpdateTransactionAsync(Guid userId, UpdateTransactionRequest request)
        {
            return await _transactionUpdateHandler.HandleAsync(userId, request);
        }

        public async Task<Result<bool>> DeleteTransactionAsync(Guid userId, DeleteTransactionRequest request)
        {
            return await _transactionDeleteHandler.HandleAsync(userId, request);
        }

        public async Task<Result<PaginatedTransactionListResponse>> GetTransactionsFromLedgerAsync(GetTransactionsFromLedgerRequest request)
        {
            // If ledger is specified, check access
            if (request.LedgerId.HasValue)
            {
                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(request.UserId, request.LedgerId.Value);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
                }
            }

            var (items, totalCount) = await _transactionQueryService.GetTransactionListPaginatedAsync(
                request.LedgerId,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.UserId,
                request.Page,
                request.PageSize
            );

            if (items is null)
            {
                return Result<PaginatedTransactionListResponse>.NotFound("Transactions not found");
            }

            var summary = await _transactionQueryService.GetTransactionSummaryAsync(request.LedgerId,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.UserId);

            var response = new PaginatedTransactionListResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Summary = summary
            };

            return Result<PaginatedTransactionListResponse>.Success(response);
        }

        public async Task<Result<GetCreateTransactionPageDataResponse>> GetCreateTransactionPageDataAsync(Guid userId, Guid? budgetId, Guid? ledgerId)
        {
            //var ledgers = await _ledgerQueryService.GetUserLedgersAsync(userId);
            var accounts = await _accountQueryService.GetAccountsAsync(userId);
            var transactionTypes = await _transactionTypeQueryService.GetTransactionsTypesAsync();

            Guid existingLedgerId = Guid.Empty;

            if (ledgerId is null)
            {
                var firstLedger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId);
                if (firstLedger is not null)
                {
                    existingLedgerId = firstLedger.Id;
                }
                else
                {
                    return Result.NotFound("No ledgers found for the user");
                }
            }
            else
            {
                existingLedgerId = ledgerId.Value;
            }

            var budgets = await _budgetQueryService.GetBudgetsFromLedgerAsync(existingLedgerId);

            Dictionary<Guid, CategoryWithTypeDto> budgetCategories = new Dictionary<Guid, CategoryWithTypeDto>();
            Dictionary<Guid, CategoryWithTypeDto> transactionCategories = new Dictionary<Guid, CategoryWithTypeDto>();

            if (budgetId is not null)
            {
                var categories = await _budgetCategoryQueryService.GetBudgetCategoriesAsync(existingLedgerId, budgetId.Value);
                budgetCategories = categories.ToDictionary(x => x.Id, x => new CategoryWithTypeDto {
                    Title = x.CategoryName,
                    TransactionTypeId = x.TransactionTypeId,
                    TransactionTypeTitle = x.TransactionTypeTitle
                });
            }
            else
            {
                var categories = await _categoryQueryService.GetCategoriesAsync(existingLedgerId);
                transactionCategories = categories.ToDictionary(x => x.Id, x => new CategoryWithTypeDto {
                    Title = x.Title,
                    TransactionTypeId = x.TransactionTypeId,
                    TransactionTypeTitle = "" // If you have the title, set it here, otherwise leave blank
                });
            }

            var response = new GetCreateTransactionPageDataResponse
            {
                //Ledgers = ledgers?.ToDictionary(x => x.Id, x => x.Title),
                Accounts = accounts?.ToDictionary(x => x.AccountId, x => string.Join('|', x.AccountTitle, x.CurrencySymbol)),
                TransactionCategories = transactionCategories,
                BudgetCategories = budgetCategories,
                //Currencies = currencies?.ToDictionary(x => x.Id, x => string.Join(' ', x.Code, x.Symbol)),
                TransactionTypes = transactionTypes?.ToDictionary(x => x.Id, x=> x.Title),
                Budgets = budgets?.ToDictionary(x => x.Id, x => $"{x.StartDate.ToString("yyyy-MM-dd")}-{x.EndDate.ToString("yyyy-MM-dd")}" ),
            };

            return Result.Success(response);
        }

        public async Task<Result<Guid>> TransferAsync(Guid userId, TransferTransactionRequest request)
        {
            return await _transactionTransferHandler.HandleAsync(userId, request);
        }

        public async Task<Result<int>> ImportTransactionsAsync(Guid userId, Guid ledgerId, Microsoft.AspNetCore.Http.IFormFile file)
        {
            return await _transactionImportHandler.HandleAsync(userId, ledgerId, file);
        }

    }
}
