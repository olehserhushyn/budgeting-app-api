namespace FamilyBudgeting.Domain.Data.Users
{
    public interface IIdentityRepository
    {
        Task<ApplicationUser> RegisterAsync(string firstName, string lastName, string email, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<ApplicationUser?> FindByEmailAsync(string email);
    }
}
