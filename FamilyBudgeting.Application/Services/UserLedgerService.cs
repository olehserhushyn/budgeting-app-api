using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.UserLedgers;
using FamilyBudgeting.Domain.DTOs.Requests.UserLedgers;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.UserLedgers;
using FamilyBudgeting.Domain.Data.ValueObjects;

namespace FamilyBudgeting.Domain.Services
{
    public class UserLedgerService : IUserLedgerService
    {
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly IUserLedgerRepository _userLedgerRepository;

        public UserLedgerService(IUserLedgerQueryService userLedgerQueryService, IUserLedgerRepository userLedgerRepository)
        {
            _userLedgerQueryService = userLedgerQueryService;
            _userLedgerRepository = userLedgerRepository;
        }

        public async Task<Result> CheckUserLedgerAccess(Guid userId, Guid ledgerId)
        {
            bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, ledgerId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this ledger");
            }

            return Result.Success();
        }

        public async Task<Result<IEnumerable<UserLedgerDetailsDto>>> GetLedgerUsersAsync(Guid userId, Guid ledgerId)
        {
            bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, ledgerId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this ledger");
            }

            return Result.Success(await _userLedgerQueryService.GetUserLedgerDtoByLedgerIdAsync(ledgerId));
        }

        public async Task<Result> UpdateUserLedgerAsync(Guid userId, UpdateUserLedgerRequest request)
        {
            bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, request.LedgerId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this ledger");
            }

            // check if user is an admin
            bool canUpdate = await _userLedgerQueryService.IsUserInLedgerRoleAsync(userId, request.LedgerId, UserLedgerRoles.Owner, UserLedgerRoles.Administrator);

            if (!canUpdate)
            {
                return Result.Forbidden("You cannot update users within this ledger");
            }

            // 1 get existing ledger
            var uLedgerDto = await _userLedgerQueryService.GetUserLedgerDtoByUserIdAsync(request.LedgerId, request.UserId);

            if (uLedgerDto is null)
            {
                return Result.NotFound("User not found for this ledger");
            }

            // 2 update it
            UserLedger uLedger = new UserLedger(uLedgerDto.UserId, uLedgerDto.RoleId, uLedgerDto.LedgerId);
            uLedger.Update(request.RoleId);

            // 3 save to db
            bool updated = await _userLedgerRepository.UpdateUserLedgerAsync(uLedgerDto.Id, uLedger);
            if (!updated)
            {
                return Result.Error("Unable to update user in the ledger");
            }

            return Result.Success();
        }

        public async Task<Result> DeleteUserLedgerAsync(Guid userId, DeleteUserLedgerRequest request)
        {
            bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, request.LedgerId);

            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this ledger");
            }

            // check if user is an admin
            bool canUpdate = await _userLedgerQueryService.IsUserInLedgerRoleAsync(userId, request.LedgerId, UserLedgerRoles.Owner, UserLedgerRoles.Administrator);

            if (!canUpdate)
            {
                return Result.Forbidden("You cannot update users within this ledger");
            }

            // 1 get existing ledger
            var uLedgerDto = await _userLedgerQueryService.GetUserLedgerDtoByUserIdAsync(request.LedgerId, request.UserId);

            if (uLedgerDto is null)
            {
                return Result.NotFound("User not found for this ledger");
            }

            // 2 update it
            UserLedger uLedger = new UserLedger(uLedgerDto.UserId, uLedgerDto.RoleId, uLedgerDto.LedgerId);
            uLedger.Delete();

            // 3 save to db
            bool updated = await _userLedgerRepository.UpdateUserLedgerAsync(uLedgerDto.Id, uLedger);
            if (!updated)
            {
                return Result.Error("Unable to update user in the ledger");
            }

            return Result.Success();
        }
    }
}
