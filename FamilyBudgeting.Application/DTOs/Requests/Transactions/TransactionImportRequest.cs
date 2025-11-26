using Microsoft.AspNetCore.Http;

namespace FamilyBudgeting.Application.DTOs.Requests.Transactions
{
    public class TransactionImportRequest
    {
        public Guid LedgerId { get; set; }
        public IFormFile File { get; set; }
    }
}
