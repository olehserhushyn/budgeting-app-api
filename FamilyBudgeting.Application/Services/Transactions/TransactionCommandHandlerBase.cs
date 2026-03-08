using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Domain.Services
{
    public abstract class TransactionCommandHandlerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        protected TransactionCommandHandlerBase(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> operation, string? errorLogMessage = null)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await operation();
                if (result.IsSuccess)
                    await _unitOfWork.CommitTransactionAsync();
                else
                    await _unitOfWork.RollbackTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, string.IsNullOrWhiteSpace(errorLogMessage)
                    ? "Unhandled exception in transactional command handler"
                    : errorLogMessage);
                throw;
            }
        }

        protected async Task<Result<T>> ExecuteInTransactionWithErrorAsync<T>(Func<Task<Result<T>>> operation, string errorLogMessage)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await operation();
                if (result.IsSuccess)
                    await _unitOfWork.CommitTransactionAsync();
                else
                    await _unitOfWork.RollbackTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, errorLogMessage);
                return Result<T>.Error(ex.Message);
            }
        }
    }
}
