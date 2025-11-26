using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.UserBudgets;
using FamilyBudgeting.Domain.DTOs.Requests.UserLedgers;
using FamilyBudgeting.Domain.DTOs.Responses.UserBudgets;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IUserBudgetService
    {
        Task<Result<GetBudgetUsersResponse>> GetBudgetUsersAsync(Guid userId, Guid budgetId);
        Task<Result> UpdateUserBudgetAsync(Guid userId, UpdateUserBudgetRequest request);
        Task<Result> DeleteUserBudgetAsync(Guid userId, DeleteUserBudgetRequest request);
    }
}
