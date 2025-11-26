using FamilyBudgeting.Domain.DTOs.Models.TransactionTypes;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface ITransactionTypeQueryService
    {
        Task<IEnumerable<TransactionTypeDto>> GetTransactionsTypesAsync();
        Task<TransactionTypeDto?> GetTransactionTypeAsync(Guid transactionTypeId);
    }
}
