using Ardalis.Result;
using FamilyBudgeting.Domain.Data.UsersSettings;
using FamilyBudgeting.Domain.Data.Ledgers;
using FamilyBudgeting.Domain.Data.UserLedgers;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.ValueObjects;
using FamilyBudgeting.Domain.Core;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Application.Services
{
    public class UserSetupService : IUserSetupService
    {
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly ILedgerRepository _ledgerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountTypeQueryService _accountTypeQueryService;
        private readonly ICurrencyQueryService _currencyQueryService;
        private readonly IUserLedgerRoleQueryService _userLedgerRoleQueryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserSetupService> _logger;

        public UserSetupService(
            IUserSettingsRepository userSettingsRepository,
            ILedgerRepository ledgerRepository,
            IAccountRepository accountRepository,
            IAccountTypeQueryService accountTypeQueryService,
            ICurrencyQueryService currencyQueryService,
            IUserLedgerRoleQueryService userLedgerRoleQueryService,
            IUnitOfWork unitOfWork,
            ILogger<UserSetupService> logger)
        {
            _userSettingsRepository = userSettingsRepository;
            _ledgerRepository = ledgerRepository;
            _accountRepository = accountRepository;
            _accountTypeQueryService = accountTypeQueryService;
            _currencyQueryService = currencyQueryService;
            _userLedgerRoleQueryService = userLedgerRoleQueryService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> SetupDefaultUserDataAsync(Guid userId, string firstName)
        {
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync();

                // Get USD currency
                var usdCurrency = await _currencyQueryService.GetCurrencyByCodeAsync("USD");
                if (usdCurrency == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("USD currency not found in system");
                }

                // Get default account type (first available)
                var accountTypes = await _accountTypeQueryService.GetAccountTypes();
                var defaultAccountType = accountTypes?.FirstOrDefault();
                if (defaultAccountType == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("No account types found in system");
                }

                // Get owner role
                var ownerRole = await _userLedgerRoleQueryService.GetUserLedgerRoleByTitleAsync(UserLedgerRoles.Owner);
                if (ownerRole == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Failed to get owner role");
                }

                // Create UserSettings (show onboarding by default)
                var userSettings = new UserSettings(userId, usdCurrency.Id, true);
                var userSettingsId = await _userSettingsRepository.CreateUserSettingsAsync(userSettings);
                if (userSettingsId == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Failed to create user settings");
                }

                // Create default ledger
                var ledger = new Ledger($"{firstName}'s Personal Ledger");
                var userLedger = new UserLedger(userId, ownerRole.Id, Guid.Empty);
                var (ledgerId, userLedgerId) = await _ledgerRepository.CreateLedgerAsync(ledger, userLedger);
                if (ledgerId == Guid.Empty || userLedgerId == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Failed to create default ledger");
                }

                // Create default account
                var account = new Account(userId, defaultAccountType.Id, "Main Account", 0, usdCurrency.Id);
                var accountId = await _accountRepository.CreateAccountAsync(account);
                if (accountId == Guid.Empty)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Failed to create default account");
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created default data for user {UserId}: UserSettings={UserSettingsId}, Ledger={LedgerId}, Account={AccountId}", 
                    userId, userSettingsId, ledgerId, accountId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // Rollback transaction on any exception
                try
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback transaction for user {UserId}", userId);
                }

                _logger.LogError(ex, "Failed to setup default user data for user {UserId}", userId);
                return Result.Error($"Failed to setup default user data: {ex.Message}");
            }
        }
    }
}
