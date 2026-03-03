using FamilyBudgeting.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyBudgeting.Domain.Services
{
    public static class TransactionModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddTransactionModule(this IServiceCollection services)
        {
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionCreateHandler, TransactionCreateHandler>();
            services.AddScoped<ITransactionUpdateHandler, TransactionUpdateHandler>();
            services.AddScoped<ITransactionDeleteHandler, TransactionDeleteHandler>();
            services.AddScoped<ITransactionTransferHandler, TransactionTransferHandler>();
            services.AddScoped<ITransactionImportHandler, TransactionImportHandler>();
            services.AddScoped<ITransactionListQueryHandler, TransactionListQueryHandler>();
            services.AddScoped<ITransactionPageDataQueryHandler, TransactionPageDataQueryHandler>();

            return services;
        }
    }
}
