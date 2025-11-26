using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.AccountTypes;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IAccountTypeService
    {
        Task<Result<IEnumerable<AccountTypeDto>>> GetAccountTypesAsync();
        Task<Result<Guid>> CreateAccountTypeAsync(string title);
        Task<Result<AccountTypeDto?>> GetAccountTypeEntityAsync(Guid accountTypeId);
    }
}
