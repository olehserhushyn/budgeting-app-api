namespace FamilyBudgeting.Domain.Data.Transactions
{
    public interface ITransactionRepository
    {
        Task<Guid> CreateTransactionAsync(Transaction transaction);
        Task<bool> UpdateTransactionAsync(Guid id, Transaction transaction);
        Task<int> CreateTransactionsAsync(IEnumerable<Transaction> transactions);
    }
}
