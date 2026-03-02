using Ardalis.Result;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionImportHandler
    {
        Task<Result<int>> HandleAsync(Guid userId, Guid ledgerId, Microsoft.AspNetCore.Http.IFormFile file);
    }
}
