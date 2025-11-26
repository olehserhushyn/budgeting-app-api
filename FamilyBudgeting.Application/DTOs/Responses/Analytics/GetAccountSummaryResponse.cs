namespace FamilyBudgeting.Application.DTOs.Responses.Analytics
{
    public class GetAccountSummaryResponse
    {
        // Represents the total assets value
        public decimal TotalAssets { get; set; }

        // Contains the distribution of balances across different account types
        public Dictionary<string, decimal> BalanceDistribution { get; set; } = new Dictionary<string, decimal>();

        // Holds performance metrics grouped by account type
        public Dictionary<string, AccountTypePerformance> PerformanceByType { get; set; } = new Dictionary<string, AccountTypePerformance>();
    }

    // Class to encapsulate performance details for each account type
    public class AccountTypePerformance
    {
        // The total amount for this account type
        public decimal TotalAmount { get; set; }

        // The percentage of total assets this type represents
        public decimal PercentageOfTotal { get; set; }

        // Dictionary of individual accounts and their performance
        public Dictionary<string, AccountPerformance> Accounts { get; set; }
    }

    // Class to encapsulate individual account performance details
    public class AccountPerformance
    {
        // The current amount in the account
        public decimal Amount { get; set; }

        // The growth percentage over the last 12 months
        public decimal GrowthPercentage { get; set; }
    }
}
