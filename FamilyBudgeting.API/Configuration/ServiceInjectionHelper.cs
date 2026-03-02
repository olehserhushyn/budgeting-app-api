using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.AccountTypes;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Domain.Data.Budgets;
using FamilyBudgeting.Domain.Data.Categories;
using FamilyBudgeting.Domain.Data.Invitations;
using FamilyBudgeting.Domain.Data.Ledgers;
using FamilyBudgeting.Domain.Data.Subcategories;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.Data.UserBudgets;
using FamilyBudgeting.Domain.Data.UserLedgers;
using FamilyBudgeting.Domain.Data.Users;
using FamilyBudgeting.Domain.Data.UsersSettings;
using FamilyBudgeting.Domain.Interfaces;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Application.Services;
using FamilyBudgeting.Infrastructure.Context;
using FamilyBudgeting.Infrastructure.Core;
using FamilyBudgeting.Infrastructure.JwtProviders;
using FamilyBudgeting.Infrastructure.Queries;
using FamilyBudgeting.Infrastructure.Repositories;
using FamilyBudgeting.Infrastructure.Repositories.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyBudgeting.API.Configuration
{
    public static class ServiceInjectionHelper
    {
        public static void InjectRepositories(this IServiceCollection services)
        {
            services.AddScoped<ILedgerRepository, LedgerRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IUserLedgerRepository, UserLedgerRepository>();
            services.AddScoped<IUserBudgetRepository, UserBudgetRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBudgetCategoryRepository, BudgetCategoryRepository>();
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();

            // ef
            services.AddScoped<IIdentityRepository, IdentityRepository>();
        }

        public static void InjectQueryServices(this IServiceCollection services)
        {
            services.AddScoped<IAccessQueryService, AccessQueryService>();

            services.AddScoped<IUserLedgerRoleQueryService, UserLedgerRoleQueryService>();
            services.AddScoped<IUserQueryService, UserQueryService>();
            services.AddScoped<IAccountTypeQueryService, AccountTypeQueryService>();
            services.AddScoped<IAccountQueryService, AccountQueryService>();
            services.AddScoped<ILedgerQueryService, LedgerQueryService>();
            services.AddScoped<ITransactionQueryService, TransactionQueryService>();
            services.AddScoped<IBudgetQueryService, BudgetQueryService>();
            services.AddScoped<ISubcategoryQueryService, SubcategoryQueryService>();
            services.AddScoped<IUserLedgerQueryService, UserLedgerQueryService>();
            services.AddScoped<ICurrencyQueryService, CurrencyQueryService>();
            services.AddScoped<ITransactionTypeQueryService, TransactionTypeQueryService>();
            services.AddScoped<ICategoryQueryService, CategoryQueryService>();
            services.AddScoped<IBudgetCategoryQueryService, BudgetCategoryQueryService>();
            services.AddScoped<IDashboardQueryService, DashboardQueryService>();
            services.AddScoped<IInvitationQueryService, InvitationQueryService>();
            services.AddScoped<IUserBudgetQueryService, UserBudgetQueryService>();
            services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
            services.AddScoped<IUserSettingsQueryService, UserSettingsQueryService>();
        }

        public static void InjectServices(this IServiceCollection services)
        {
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserLedgerRoleService, UserLedgerRoleService>();
            services.AddScoped<IAccountTypeService, AccountTypeService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ILedgerService, LedgerService>();
            services.AddScoped<IUserLedgerService, UserLedgerService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionCreateHandler, TransactionCreateHandler>();
            services.AddScoped<ITransactionUpdateHandler, TransactionUpdateHandler>();
            services.AddScoped<ITransactionDeleteHandler, TransactionDeleteHandler>();
            services.AddScoped<ITransactionTransferHandler, TransactionTransferHandler>();
            services.AddScoped<ITransactionImportHandler, TransactionImportHandler>();
            services.AddScoped<ITransactionListQueryHandler, TransactionListQueryHandler>();
            services.AddScoped<ITransactionPageDataQueryHandler, TransactionPageDataQueryHandler>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISubcategoryService, SubcategoryService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
            services.AddScoped<ITransactionTypeService, TransactionTypeService>();
            services.AddScoped<IAppInitializer, AppInitializer>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IUserBudgetService, UserBudgetService>();
            services.AddScoped<IAnalyticsService, FamilyBudgeting.Application.Services.AnalyticsService>();
            services.AddScoped<IUserSetupService, UserSetupService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
        }
    }
}
