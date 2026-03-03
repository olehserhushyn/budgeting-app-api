using FamilyBudgeting.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyBudgeting.Domain.Services
{
    public static class TransactionModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactionModule(this IServiceCollection services)
        {
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddTransactionPolicies();
            services.AddTransactionCommandHandlers();
            services.AddTransactionQueryHandlers();

            return services;
        }

        public static IServiceCollection AddTransactionPolicies(this IServiceCollection services)
        {
            services.AddScoped<ITransactionAccessPolicy, TransactionAccessPolicy>();
            services.AddScoped<ITransactionCategoryResolutionPolicy, TransactionCategoryResolutionPolicy>();
            return services;
        }

        public static IServiceCollection AddTransactionCommandHandlers(this IServiceCollection services)
        {
            services.AddScoped<ITransactionCreateHandler, TransactionCreateHandler>();
            services.AddScoped<ITransactionUpdateHandler, TransactionUpdateHandler>();
            services.AddScoped<ITransactionDeleteHandler, TransactionDeleteHandler>();
            services.AddScoped<ITransactionTransferHandler, TransactionTransferHandler>();
            services.AddScoped<ITransactionImportHandler, TransactionImportHandler>();

            return services;
        }

        public static IServiceCollection AddTransactionQueryHandlers(this IServiceCollection services)
        {
            services.AddScoped<ITransactionListQueryHandler, TransactionListQueryHandler>();
            services.AddScoped<ITransactionPageDataQueryHandler, TransactionPageDataQueryHandler>();

            return services;
        }
    }
}
