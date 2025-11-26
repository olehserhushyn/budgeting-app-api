using FamilyBudgeting.Domain.Constants;

namespace FamilyBudgeting.Domain.Utilities
{
    public static class TransactionHelper
    {
        public static int AdjustCentsSign(int centsAmount, string transactionType)
        {
            switch (transactionType)
            {
                case TransactionTypes.Income:
                    break;
                case TransactionTypes.Investment:
                    throw new ArgumentException($"Not Implemented transaction type: {transactionType}");
                case TransactionTypes.Withdrawal:
                    throw new ArgumentException($"Not Implemented transaction type: {transactionType}");
                case TransactionTypes.Transfer:
                    throw new ArgumentException($"Not Implemented transaction type: {transactionType}");
                case TransactionTypes.Expense:
                    centsAmount *= -1; // Adjust sign for expenses
                    break;
                default:
                    throw new ArgumentException($"Invalid transaction type: {transactionType}");
            }

            return centsAmount;
        }
    }
}
