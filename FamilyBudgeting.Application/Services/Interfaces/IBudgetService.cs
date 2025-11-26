using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Budgets;
using FamilyBudgeting.Domain.DTOs.Requests.Budgets;
using FamilyBudgeting.Domain.DTOs.Responses.Budgets;
using FamilyBudgeting.Domain.Data.Budgets;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IBudgetService
    {
        Task<Result<Guid>> CreateBudgetAsync(CreateBudgetRequest request);
        Task<Result<IEnumerable<GetLedgerBudgetsResponse>>> GetBudgetsFromLedgerAsync(Guid ledgerId);
        Task<Result<Guid>> GetLedgerIdFromBudgetAsync(Guid budgetId);
        Task<Result<BudgetDto>> GetBudgetAsync(Guid budgetId);
        Task<Result<GetBudgetDetailsResponse>> GetBudetDetailsAsync(Guid budgetId);
        Task<Result<Guid>> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request);
        Task<Result> DeleteBudgetAsync(Guid budgetId);
    }
}
