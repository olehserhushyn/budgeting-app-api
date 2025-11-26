namespace FamilyBudgeting.Domain.DTOs.Models.Queries.DashboardDtos
{
    public class MonthlyFlowResult
    {
        public int? Amount { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyTitle { get; set; }
        public int CurrencyFractionalUnitFactor { get; set; }
    }
}
