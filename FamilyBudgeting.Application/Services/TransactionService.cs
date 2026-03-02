using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionCreateHandler _transactionCreateHandler;
        private readonly ITransactionUpdateHandler _transactionUpdateHandler;
        private readonly ITransactionDeleteHandler _transactionDeleteHandler;
        private readonly ITransactionTransferHandler _transactionTransferHandler;
        private readonly ITransactionImportHandler _transactionImportHandler;
        private readonly ITransactionListQueryHandler _transactionListQueryHandler;
        private readonly ITransactionPageDataQueryHandler _transactionPageDataQueryHandler;

        public TransactionService(
            ITransactionCreateHandler transactionCreateHandler,
            ITransactionUpdateHandler transactionUpdateHandler,
            ITransactionDeleteHandler transactionDeleteHandler,
            ITransactionTransferHandler transactionTransferHandler,
            ITransactionImportHandler transactionImportHandler,
            ITransactionListQueryHandler transactionListQueryHandler,
            ITransactionPageDataQueryHandler transactionPageDataQueryHandler)
        {
            _transactionCreateHandler = transactionCreateHandler;
            _transactionUpdateHandler = transactionUpdateHandler;
            _transactionDeleteHandler = transactionDeleteHandler;
            _transactionTransferHandler = transactionTransferHandler;
            _transactionImportHandler = transactionImportHandler;
            _transactionListQueryHandler = transactionListQueryHandler;
            _transactionPageDataQueryHandler = transactionPageDataQueryHandler;
        }

        public async Task<Result<Guid>> CreateTransactionAsync(Guid userId, CreateTransactionRequest request)
            => await _transactionCreateHandler.HandleAsync(userId, request);

        public async Task<Result<bool>> UpdateTransactionAsync(Guid userId, UpdateTransactionRequest request)
            => await _transactionUpdateHandler.HandleAsync(userId, request);

        public async Task<Result<bool>> DeleteTransactionAsync(Guid userId, DeleteTransactionRequest request)
            => await _transactionDeleteHandler.HandleAsync(userId, request);

        public async Task<Result<PaginatedTransactionListResponse>> GetTransactionsFromLedgerAsync(GetTransactionsFromLedgerRequest request)
            => await _transactionListQueryHandler.HandleAsync(request);

        public async Task<Result<GetCreateTransactionPageDataResponse>> GetCreateTransactionPageDataAsync(Guid userId, Guid? budgetId, Guid? ledgerId)
            => await _transactionPageDataQueryHandler.HandleAsync(userId, budgetId, ledgerId);

        public async Task<Result<Guid>> TransferAsync(Guid userId, TransferTransactionRequest request)
            => await _transactionTransferHandler.HandleAsync(userId, request);

        public async Task<Result<int>> ImportTransactionsAsync(Guid userId, Guid ledgerId, Microsoft.AspNetCore.Http.IFormFile file)
            => await _transactionImportHandler.HandleAsync(userId, ledgerId, file);
    }
}
