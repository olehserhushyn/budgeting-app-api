using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.Interfaces;
using FamilyBudgeting.Domain.Interfaces.Queries;

namespace FamilyBudgeting.Infrastructure.Core
{
    public class AppInitializer : IAppInitializer
    {
        private readonly ITransactionTypeQueryService _queryService;

        public AppInitializer(ITransactionTypeQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task InitializeAsync()
        {
            var types = await _queryService.GetTransactionsTypesAsync();
            TransactionTypes.Initialize(types);
        }
    }
}
