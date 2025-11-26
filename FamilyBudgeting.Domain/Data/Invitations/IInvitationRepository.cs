namespace FamilyBudgeting.Domain.Data.Invitations
{
    public interface IInvitationRepository
    {
        Task<Guid> CreateAsync(Invitation invitation);
        Task<bool> UpdateAsync(Invitation invitation);
    }
} 