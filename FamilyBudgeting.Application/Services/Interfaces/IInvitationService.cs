using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Invitations;
using FamilyBudgeting.Domain.DTOs.Requests.Invitations;
using FamilyBudgeting.Domain.DTOs.Requests.Ledgers;
using FamilyBudgeting.Domain.DTOs.Requests.Budgets;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IInvitationService
    {
        Task<Result<string>> CreateLedgerInvitationAsync(CreateLedgerInvitationRequest request, Guid inviterUserId);
        Task<Result<string>> CreateBudgetInvitationAsync(CreateBudgetInvitationRequest request, Guid inviterUserId);
        Task<Result<InvitationDto>> GetByTokenAsync(string token);
        Task<Result<GetPendingInvitationsResponse>> GetPendingByEmailAsync(string email);
        Task<Result> AcceptInvitationAsync(string token, Guid userId, string userEmail);
        Task<Result> DeclineInvitationAsync(string token, string userEmail);
    }
} 