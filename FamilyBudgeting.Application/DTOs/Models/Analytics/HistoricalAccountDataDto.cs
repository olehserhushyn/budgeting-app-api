namespace FamilyBudgeting.Application.DTOs.Models.Analytics
{
    public class HistoricalAccountDataDto
    {
        public Guid AccountId { get; set; }
        public string AccountTitle { get; set; }
        public string AccountTypeName { get; set; }
        public decimal NetChange { get; set; }
    }
}
