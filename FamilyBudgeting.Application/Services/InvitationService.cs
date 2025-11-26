using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Invitations;
using FamilyBudgeting.Domain.DTOs.Requests.Invitations;
using FamilyBudgeting.Domain.DTOs.Requests.Ledgers;
using FamilyBudgeting.Domain.DTOs.Requests.Budgets;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.Invitations;
using FamilyBudgeting.Domain.Data.UserLedgers;
using FamilyBudgeting.Domain.Data.UserBudgets;
using FamilyBudgeting.Domain.Data.ValueObjects;

namespace FamilyBudgeting.Domain.Services
{
    public class InvitationService : IInvitationService
    {
        private const int InvitationLastDays = 7;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IAccessQueryService _accessQueryService;
        private readonly IInvitationQueryService _invitationQueryService;
        private readonly IUserLedgerRepository _userLedgerRepository;
        private readonly IUserBudgetRepository _userBudgetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public InvitationService(IInvitationRepository invitationRepository,
            IAccessQueryService accessQueryService, IInvitationQueryService invitationQueryService, 
            IUserLedgerRepository userLedgerRepository, IUserBudgetRepository userBudgetRepository, IUnitOfWork unitOfWork)
        {
            _invitationRepository = invitationRepository;
            _accessQueryService = accessQueryService;
            _invitationQueryService = invitationQueryService;
            _userLedgerRepository = userLedgerRepository;
            _userBudgetRepository = userBudgetRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> CreateLedgerInvitationAsync(CreateLedgerInvitationRequest request, Guid inviterUserId)
        {
            bool isAdmin = await _accessQueryService.UserHasLedgerRolesAsync(inviterUserId, request.LedgerId, UserLedgerRoles.Owner, UserLedgerRoles.Administrator);

            if (!isAdmin)
            {
                return Result<string>.Forbidden("Only ledger admins can invite users.");
            }

            var invitation = new Invitation(request.InvitedEmail, request.InvitedRoleId, inviterUserId, 
                DestinationType.Ledger, request.LedgerId, InvitationStatus.Pending, Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(InvitationLastDays));

            await _invitationRepository.CreateAsync(invitation);
            return Result<string>.Success(invitation.Token);
        }

        public async Task<Result<string>> CreateBudgetInvitationAsync(CreateBudgetInvitationRequest request, Guid inviterUserId)
        {
            bool hasAccess = await _accessQueryService.UserHasAccessToBudgetAsync(inviterUserId, request.BudgetId);

            if (!hasAccess)
            {
                return Result<string>.Forbidden("You don't have access to this budget.");
            }

            var invitation = new Invitation(request.InvitedEmail, request.InvitedRoleId, inviterUserId, 
                DestinationType.Budget, request.BudgetId, InvitationStatus.Pending, Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(InvitationLastDays));

            await _invitationRepository.CreateAsync(invitation);
            return Result<string>.Success(invitation.Token);
        }

        public async Task<Result<InvitationDto>> GetByTokenAsync(string token)
        {
            var invitation = await _invitationQueryService.GetByTokenAsync(token);
            if (invitation is null)
            {
                return Result<InvitationDto>.NotFound();
            }
            return Result<InvitationDto>.Success(invitation);
        }

        public async Task<Result<GetPendingInvitationsResponse>> GetPendingByEmailAsync(string email)
        {
            var invitations = await _invitationQueryService.GetPendingByEmailAsync(email);

            var response = new GetPendingInvitationsResponse();
            response.LedgerInvitations = invitations.Where(x => x.DestinationType == DestinationType.Ledger).ToList();
            response.BudgetInvitations = invitations.Where(x => x.DestinationType == DestinationType.Budget).ToList();

            return Result<GetPendingInvitationsResponse>.Success(response);
        }

        public async Task<Result> AcceptInvitationAsync(string token, Guid userId, string userEmail)
        {
            var existingInvite = await _invitationQueryService.GetByTokenAsync(token);

            if (existingInvite is null)
            {
                return Result.NotFound("Invitation not found or expired");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                Invitation invitation = new Invitation(existingInvite.InvitedEmail, existingInvite.InvitedRoleId,
               existingInvite.InviterUserId, existingInvite.DestinationType,
               existingInvite.DestinationId, existingInvite.Status, existingInvite.Token,
               existingInvite.ExpiresAt);

                invitation.AcceptInvite(userEmail);

                var success = await _invitationRepository.UpdateAsync(invitation);

                // add user to either UserLedger or UserBudget
                switch (invitation.DestinationType)
                {
                    case DestinationType.Ledger:
                        UserLedger uLedger = new UserLedger(userId, invitation.InvitedRoleId, invitation.DestinationId);
                        await _userLedgerRepository.CreateUserLedgerAsync(uLedger);
                        break;
                    case DestinationType.Budget:
                        UserBudget userBudget = new UserBudget(userId, invitation.InvitedRoleId, invitation.DestinationId);
                        await _userBudgetRepository.CreateUserBudgetAsync(userBudget);
                        break;
                    default:
                        throw new Exception($"Unrecognized Destination Type in AcceptInvitationAsync: {invitation.DestinationType}");
                }

                await _unitOfWork.CommitTransactionAsync();

                return Result.Success();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Result> DeclineInvitationAsync(string token, string userEmail)
        {
            var existingInvite = await _invitationQueryService.GetByTokenAsync(token);

            if (existingInvite is null)
            {
                return Result.NotFound("Invitation not found or expired");
            }

            Invitation invitation = new Invitation(existingInvite.InvitedEmail, existingInvite.InvitedRoleId,
                existingInvite.InviterUserId, existingInvite.DestinationType,
                existingInvite.DestinationId, existingInvite.Status, existingInvite.Token,
                existingInvite.ExpiresAt);

            invitation.DeclineInvite(userEmail);

            var success = await _invitationRepository.UpdateAsync(invitation);

            if (!success)
            {
                return Result.Error("Unable to accept invitation.");
            }

            return Result.Success();
        }
    }
} 