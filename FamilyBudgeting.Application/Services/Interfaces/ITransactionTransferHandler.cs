using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionTransferHandler
    {
        Task<Result<Guid>> HandleAsync(Guid userId, TransferTransactionRequest request);
    }
}
