using FamilyBudgeting.Domain.DTOs.Models.TransactionTypes;

namespace FamilyBudgeting.Domain.Constants
{
    public static class TransactionTypes
    {
        public const string Income = "Income";
        public const string Investment = "Investment";
        public const string Expense = "Expense";
        public const string Withdrawal = "Withdrawal";
        public const string Transfer = "Transfer";

        public static Guid IncomeId { get; private set; }
        public static Guid InvestmentId { get; private set; }
        public static Guid ExpenseId { get; private set; }
        public static Guid WithdrawalId { get; private set; }
        public static Guid TransferId { get; private set; }

        public static void Initialize(IEnumerable<TransactionTypeDto> types)
        {
            IncomeId = types.First(t => t.Title == TransactionTypes.Income).Id;
            InvestmentId = types.First(t => t.Title == TransactionTypes.Investment).Id;
            ExpenseId = types.First(t => t.Title == TransactionTypes.Expense).Id;
            WithdrawalId = types.First(t => t.Title == TransactionTypes.Withdrawal).Id;
            TransferId = types.First(t => t.Title == TransactionTypes.Transfer).Id;
        }

        public static bool IsIncome(Guid id) => id == IncomeId;
        public static bool IsExpense(Guid id) => id == ExpenseId;
    }
}
