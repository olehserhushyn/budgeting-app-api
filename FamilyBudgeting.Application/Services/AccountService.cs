using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Accounts;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.Accounts;

namespace FamilyBudgeting.Domain.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountQueryService _accountQueryService;

        public AccountService(IAccountRepository accountRepository, IAccountQueryService accountQueryService)
        {
            _accountRepository = accountRepository;
            _accountQueryService = accountQueryService;
        }

        public async Task<Result<Guid>> CreateAccountAsync(Guid userId, CreateAccountRequest request)
        {
            var account = new Account(userId, request.AccountTypeId, request.Title, request.Balance, request.CurrencyId);
            Guid aId = await _accountRepository.CreateAccountAsync(account);
            return Result.Success(aId);
        }

        public async Task<Result<IEnumerable<DTOs.Models.Accounts.AccountCurrencyDetailsDto>>> GetAccountsAsync(Guid userId)
        {
            var accounts = await _accountQueryService.GetAccountsAsync(userId);
            if (accounts is null)
            {
                return Result<IEnumerable<DTOs.Models.Accounts.AccountCurrencyDetailsDto>>.NotFound("Accounts not found");
            }
            return Result<IEnumerable<DTOs.Models.Accounts.AccountCurrencyDetailsDto>>.Success(accounts);
        }

        public async Task<Result<DTOs.Models.Accounts.AccountCurrencyDetailsDto>> GetAccountAsync(Guid userId, Guid accountId)
        {
            var account = await _accountQueryService.GetAccountAsync(accountId);
            if (account is null)
            {
                return Result<DTOs.Models.Accounts.AccountCurrencyDetailsDto>.NotFound("Account not found");
            }

            if (account.UserId != userId)
            {
                return Result.Unauthorized("You are not authorized to edit this Account");
            }

            return Result<DTOs.Models.Accounts.AccountCurrencyDetailsDto>.Success(account);
        }

        public async Task<Result> UpdateAccountAsync(Guid userId, UpdateAccountRequest request)
        {
            var accountDto = await _accountQueryService.GetAccountDtoAsync(request.AccountId);
            if (accountDto is null)
            {
                return Result.NotFound();
            }

            if (accountDto.UserId != userId)
            {
                return Result.Unauthorized("You are not authorized to edit this Account");
            }

            Account account = new Account(accountDto.UserId, accountDto.AccountTypeId, accountDto.Title, accountDto.Balance, accountDto.CurrencyId);
            account.Update(request.AccountTypeId, request.Title, request.Balance, request.CurrencyId);
            await _accountRepository.UpdateAccountAsync(request.AccountId, account);
            return Result.Success();
        }

        public async Task<Result> DeleteAccountAsync(Guid userId, Guid accountId)
        {
            var accountDto = await _accountQueryService.GetAccountDtoAsync(accountId);
            if (accountDto is null)
            {
                return Result.NotFound();
            }

            if (accountDto.UserId != userId)
            {
                return Result.Unauthorized("You are not authorized to delete this Account");
            }

            Account account = new Account(accountDto.UserId, accountDto.AccountTypeId, accountDto.Title, accountDto.Balance, accountDto.CurrencyId);

            account.Delete();

            await _accountRepository.UpdateAccountAsync(accountId, account);
            return Result.Success();
        }
    }
}
