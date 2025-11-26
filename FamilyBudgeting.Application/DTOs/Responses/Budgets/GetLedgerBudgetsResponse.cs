namespace FamilyBudgeting.Domain.DTOs.Responses.Budgets
{
    public class GetLedgerBudgetsResponse
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? BudgetTitle { get; set; }
    }
}
