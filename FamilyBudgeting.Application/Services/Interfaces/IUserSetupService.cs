using Ardalis.Result;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IUserSetupService
    {
        Task<Result> SetupDefaultUserDataAsync(Guid userId, string firstName);
    }
}
