using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.AccountTypes;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.AccountTypes;

namespace FamilyBudgeting.Domain.Services
{
    public class AccountTypeService : IAccountTypeService
    {
        private readonly IAccountTypeQueryService _accountTypeQueryService;
        private readonly IAccountTypeRepository _accountTypeRepository;

        public AccountTypeService(IAccountTypeQueryService accountTypeQueryService, IAccountTypeRepository accountTypeRepository)
        {
            _accountTypeQueryService = accountTypeQueryService;
            _accountTypeRepository = accountTypeRepository;
        }

        public async Task<Result<IEnumerable<AccountTypeDto>>> GetAccountTypesAsync()
        {
            var accountTypes = await _accountTypeQueryService.GetAccountTypes();
            if (accountTypes == null || !accountTypes.Any())
            {
                return Result<IEnumerable<AccountTypeDto>>.NotFound("No account types found");
            }

            return Result<IEnumerable<AccountTypeDto>>.Success(accountTypes);
        }

        public async Task<Result<Guid>> CreateAccountTypeAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Result<Guid>.Invalid(new List<ValidationError>
                {
                    new() { Identifier = "Title", ErrorMessage = "Title is required." }
                });
            }

            var accountType = new AccountType(title);
            Guid accountTypeId = await _accountTypeRepository.CreateAccountTypeAsync(accountType);
            return Result.Success(accountTypeId);
        }

        public async Task<Result<AccountTypeDto>> GetAccountTypeEntityAsync(Guid accountTypeId)
        {
            var accountType = await _accountTypeQueryService.GetAccountTypeEntity(accountTypeId);
            if (accountType is null)
            {
                return Result<AccountTypeDto>.NotFound("Account type not found");
            }

            return Result<AccountTypeDto>.Success(accountType);
        }
    }
}
