using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.UserBudgets;
using FamilyBudgeting.Domain.DTOs.Requests.UserLedgers;
using FamilyBudgeting.Domain.DTOs.Responses.UserBudgets;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.UserBudgets;
using FamilyBudgeting.Domain.Data.ValueObjects;

namespace FamilyBudgeting.Domain.Services
{
    public class UserBudgetService : IUserBudgetService
    {
        private readonly IUserBudgetQueryService _userBudgetQueryService;
        private readonly IUserBudgetRepository _userBudgetRepository;
        private readonly IAccessQueryService _accessQueryService;

        public UserBudgetService(IUserBudgetQueryService userBudgetQueryService, IUserBudgetRepository userBudgetRepository, 
            IAccessQueryService accessQueryService)
        {
            _userBudgetQueryService = userBudgetQueryService;
            _userBudgetRepository = userBudgetRepository;
            _accessQueryService = accessQueryService;
        }

        public async Task<Result<GetBudgetUsersResponse>> GetBudgetUsersAsync(Guid userId, Guid budgetId)
        {
            bool hasAccess = await _accessQueryService.UserHasAccessToBudgetAsync(userId, budgetId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this budget");
            }

            GetBudgetUsersResponse response = new GetBudgetUsersResponse();
            response.BudgetUsers = (await _userBudgetQueryService.GetUserBudgetDtoByBudgetIdAsync(budgetId)).ToList();
            response.LedgerBudgetUsers = (await _userBudgetQueryService.GetUserLedgerDtoByBudgetIdAsync(budgetId)).ToList();

            return Result.Success(response);
        }

        public async Task<Result> UpdateUserBudgetAsync(Guid userId, UpdateUserBudgetRequest request)
        {
            bool hasAccess = await _accessQueryService.UserHasAccessToBudgetAsync(userId, request.BudgetId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this Budget");
            }

            // check if user is an admin
            bool canUpdate = await _userBudgetQueryService.IsUserInLedgerRoleAsync(userId, request.BudgetId, UserLedgerRoles.Owner, UserLedgerRoles.Administrator);

            if (!canUpdate)
            {
                return Result.Forbidden("You cannot update users within this Budget");
            }

            // 1 get existing ledger
            var uLedgerDto = await _userBudgetQueryService.GetUserBudgetDtoByUserIdAsync(request.BudgetId, request.UserId);

            if (uLedgerDto is null)
            {
                return Result.NotFound("User not found for this Budget");
            }

            // 2 update it
            UserBudget uBudget = new UserBudget(uLedgerDto.UserId, uLedgerDto.RoleId, uLedgerDto.BudgetId);
            uBudget.Update(request.RoleId);

            // 3 save to db
            bool updated = await _userBudgetRepository.UpdateUserBudgetAsync(uLedgerDto.Id, uBudget);
            if (!updated)
            {
                return Result.Error("Unable to update user in the Budget");
            }

            return Result.Success();
        }

        public async Task<Result> DeleteUserBudgetAsync(Guid userId, DeleteUserBudgetRequest request)
        {
            bool hasAccess = await _accessQueryService.UserHasAccessToBudgetAsync(userId, request.BudgetId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this Budget");
            }

            // check if user is an admin
            bool canUpdate = await _userBudgetQueryService.IsUserInLedgerRoleAsync(userId, request.BudgetId, UserLedgerRoles.Owner, UserLedgerRoles.Administrator);

            if (!canUpdate)
            {
                return Result.Forbidden("You cannot update users within this Budget");
            }

            // 1 get existing ledger
            var uLedgerDto = await _userBudgetQueryService.GetUserBudgetDtoByUserIdAsync(request.BudgetId, request.UserId);

            if (uLedgerDto is null)
            {
                return Result.NotFound("User not found for this Budget");
            }

            // 2 update it
            UserBudget uBudget = new UserBudget(uLedgerDto.UserId, uLedgerDto.RoleId, uLedgerDto.BudgetId);
            uBudget.Delete();

            // 3 save to db
            bool updated = await _userBudgetRepository.UpdateUserBudgetAsync(uLedgerDto.Id, uBudget);
            if (!updated)
            {
                return Result.Error("Unable to update user in the ledger");
            }

            return Result.Success();
        }
    }
}
