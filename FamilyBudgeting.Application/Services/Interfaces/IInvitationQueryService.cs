using FamilyBudgeting.Domain.DTOs.Models.Invitations;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IInvitationQueryService
    {
        Task<bool> HasExpireAsync(string token);
        Task<InvitationDto?> GetByTokenAsync(string token);
        Task<IEnumerable<InvitationDto>> GetPendingByEmailAsync(string email);
    }
}
