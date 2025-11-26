using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.TransactionTypes;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionTypeService
    {
        Task<Result<IEnumerable<TransactionTypeDto>>> GetTransactionTypes();
    }
}
