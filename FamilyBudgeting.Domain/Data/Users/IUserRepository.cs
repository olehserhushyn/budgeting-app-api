namespace FamilyBudgeting.Domain.Data.Users
{
    public interface IUserRepository
    {
        Task<Guid> CreateUserAsync(ApplicationUser user);
    }
}
