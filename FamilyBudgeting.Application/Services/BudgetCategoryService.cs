using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.DTOs.Models.Accounts;
using FamilyBudgeting.Domain.DTOs.Models.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Domain.Data.Categories;

namespace FamilyBudgeting.Domain.Services
{
    public class BudgetCategoryService : IBudgetCategoryService
    {
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccessQueryService _accessQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrencyQueryService _currencyQueryService;
        private readonly ICategoryQueryService _categoryQueryService;

        public BudgetCategoryService(IBudgetCategoryRepository budgetCategoryRepository, IUnitOfWork unitOfWork,
            IAccessQueryService accessQueryService, IBudgetCategoryQueryService budgetCategoryQueryService,
            ILedgerQueryService ledgerQueryService, ICategoryRepository categoryRepository, ICurrencyQueryService currencyQueryService, 
            ICategoryQueryService categoryQueryService)
        {
            _budgetCategoryRepository = budgetCategoryRepository;
            _unitOfWork = unitOfWork;
            _accessQueryService = accessQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _ledgerQueryService = ledgerQueryService;
            _categoryRepository = categoryRepository;
            _currencyQueryService = currencyQueryService;
            _categoryQueryService = categoryQueryService;
        }

        public async Task<Result<BudgetCategoryDto>> GetBudgetCategoryAsync(Guid userId, Guid id)
        {
            bool hasAccess = await _accessQueryService.UserHasAccessToBudgetCategoryAsync(userId, id);
            if (hasAccess) 
            {
                return Result<BudgetCategoryDto>.Unauthorized("You do not have access to this Budget Category");
            }

            var bCategory = await _budgetCategoryQueryService.GetBudgetCategoryAsync(id);

            if (bCategory is null)
            {
                return Result<BudgetCategoryDto>.NotFound("Budget Category not found");
            }

            return Result.Success(bCategory);
        }

        public async Task<Result> UpdateBudgetCategoryAsync(Guid id, UpdateBudgetCategoryRequest request)
        {
            var oldbCategory = await _budgetCategoryQueryService.GetBudgetCategoryAsync(id);

            BudgetCategory budgetCategory = new BudgetCategory(oldbCategory.BudgetId, oldbCategory.CategoryId,
                oldbCategory.CurrencyId, oldbCategory.PlannedAmount, oldbCategory.CurrentAmount);

            int centsAmount = MoneyConverter.ConvertToCents(request.PlannedAmount, oldbCategory.CurrencyFractionalUnitFactor);

            budgetCategory.Update(oldbCategory.CategoryId, centsAmount);

            await _budgetCategoryRepository.UpdateBudgetCategoryAsync(id, budgetCategory);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<BudgetCategoriesDetailedResponse>>> GetBudgetCategoriesAsync(Guid userId, Guid budgetId)
        {
            var ledger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId, budgetId);

            if (ledger == null)
            {
                return Result.Error($"Ledger not found for budget: {budgetId}");
            }

            var categories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(ledger.Id, budgetId);
            return Result.Success(categories);
        }

        public async Task<Result<Guid>> CreateBudgetCategoryAsync(Guid userId, CreateBudgetCategoryRequest request)
        {
            // This method is still used externally, so keep transaction handling here
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var result = await CreateBudgetCategoryInternalAsync(userId, request);
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // Internal helper for use when transaction is already managed
        private async Task<Result<Guid>> CreateBudgetCategoryInternalAsync(Guid userId, CreateBudgetCategoryRequest request)
        {
            bool hasAccess = await _accessQueryService.UserHasAccessToBudgetAsync(userId, request.BudgetId);
            if (!hasAccess)
            {
                return Result.Forbidden($"User does not have access to the Budget: {request.BudgetId}");
            }

            var ledger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId, request.BudgetId);
            if (ledger is null)
            {
                return Result.NotFound($"Ledger not found with Budget Id: {request.BudgetId}");
            }

            var existingCategory = await _categoryQueryService.GetCategoryAsync(request.Title);
            var currency = await _currencyQueryService.GetCurrencyAsync(request.CurrencyId);
            if (currency is null)
            {
                return Result.NotFound($"Currency not found with Id: {request.CurrencyId}");
            }

            Guid categoryId;
            if (existingCategory is null)
            {
                var category = new Category(request.Title, ledger.Id, request.TransactionTypeId);
                categoryId = await _categoryRepository.CreateCategoryAsync(category);
            }
            else
            {
                categoryId = existingCategory.Id;
            }

            int centsPlannedAmount = MoneyConverter.ConvertToCents(request.PlannedAmount, currency.FractionalUnitFactor);
            bool isIncome = TransactionTypes.IsIncome(request.TransactionTypeId);
            int centsCurrentAmount = isIncome ? 0 : centsPlannedAmount;

            var budgetCategory = new BudgetCategory(
                request.BudgetId,
                categoryId,
                request.CurrencyId,
                centsPlannedAmount,
                centsCurrentAmount
            );

            Guid budgetCategoryId = await _budgetCategoryRepository.CreateBudgetCategoryAsync(budgetCategory);
            return Result.Success(categoryId);
        }

        public async Task<Result> DeleteBudgetCategoryAsync(Guid id)
        {
            var oldbCategory = await _budgetCategoryQueryService.GetBudgetCategoryAsync(id);
            if (oldbCategory is null)
                return Result.NotFound($"Budget Category not found: {id}");
            var budgetCategory = new BudgetCategory(oldbCategory.BudgetId, oldbCategory.CategoryId, oldbCategory.CurrencyId, oldbCategory.PlannedAmount, oldbCategory.CurrentAmount) { Id = oldbCategory.Id };
            budgetCategory.Delete();
            await _budgetCategoryRepository.UpdateBudgetCategoryAsync(id, budgetCategory);
            return Result.Success();
        }

        public async Task<Result<ImportBudgetCategoriesResponse>> ImportBudgetCategoriesAsync(Guid userId, ImportBudgetCategoriesRequest request)
        {
            // 1. Validate user access to both budgets
            bool hasAccessSource = await _accessQueryService.UserHasAccessToBudgetAsync(userId, request.SourceBudgetId);
            bool hasAccessTarget = await _accessQueryService.UserHasAccessToBudgetAsync(userId, request.TargetBudgetId);
            if (!hasAccessSource || !hasAccessTarget)
            {
                return Result.Forbidden("User does not have access to one or both budgets.");
            }

            // 2. Fetch ledgers for both budgets
            var sourceLedger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId, request.SourceBudgetId);
            var targetLedger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId, request.TargetBudgetId);
            if (sourceLedger == null || targetLedger == null)
            {
                return Result.NotFound("Source or target ledger not found.");
            }

            // 3. Fetch all categories from the source budget
            var sourceCategories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(sourceLedger.Id, request.SourceBudgetId);
            if (sourceCategories == null)
            {
                return Result.NotFound("No categories found in source budget.");
            }

            var importedCategoryIds = new List<Guid>();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                foreach (var cat in sourceCategories)
                {
                    // 4. Filter by mode
                    if (request.Mode == ImportBudgetCategoriesMode.DefaultOnly && cat.PlannedAmount != cat.CurrentAmount)
                        continue;

                    double plannedAmount = cat.PlannedAmount;
                    // carry over only for expenses
                    if (request.Mode == ImportBudgetCategoriesMode.CarryOver && cat.TransactionTypeTitle == TransactionTypes.Expense)
                    {
                        plannedAmount = cat.PlannedAmount + cat.CurrentAmount;
                    }

                    // 5. Create new category in target budget (no transaction handling here)
                    var createRequest = new CreateBudgetCategoryRequest(
                        cat.CategoryName,
                        request.TargetBudgetId,
                        cat.CurrencyId,
                        plannedAmount / (cat.CurrencyFractionalUnitFactor > 0 ? cat.CurrencyFractionalUnitFactor : 1),
                        cat.TransactionTypeId
                    );
                    var result = await CreateBudgetCategoryInternalAsync(userId, createRequest);
                    if (result.Status == ResultStatus.Ok && result.Value != Guid.Empty)
                    {
                        importedCategoryIds.Add(result.Value);
                    }
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return Result.Success(new ImportBudgetCategoriesResponse
            {
                Success = true,
                ImportedCategoryIds = importedCategoryIds
            });
        }
    }
}
