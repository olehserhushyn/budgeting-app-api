using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.TransactionTypes;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionTypeService : ITransactionTypeService
    {
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;

        public TransactionTypeService(ITransactionTypeQueryService transactionTypeQueryService)
        {
            _transactionTypeQueryService = transactionTypeQueryService;
        }

        public async Task<Result<IEnumerable<TransactionTypeDto>>> GetTransactionTypes()
        {
            var types = await _transactionTypeQueryService.GetTransactionsTypesAsync();

            return Result.Success(types);
        }
    }
}
