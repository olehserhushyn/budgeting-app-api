using FamilyBudgeting.Domain.DTOs.Models.Accounts;
using FamilyBudgeting.Domain.Data.TransactionTypes;

namespace FamilyBudgeting.Domain.DTOs.Models.Transactions
{
    public class TransactionPrerequisites
    {
        public AccountDto AccountDto { get; set; }
        public TransactionType TransactionType { get; set; }
        public int CentsAmount { get; set; }
        public int CentsAmountWithSign { get; set; }
    }
}
