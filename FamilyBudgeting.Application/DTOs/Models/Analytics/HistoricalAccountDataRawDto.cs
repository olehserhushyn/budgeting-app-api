namespace FamilyBudgeting.Application.DTOs.Models.Analytics
{
    public class HistoricalAccountDataRawDto
    {
        public Guid AccountId { get; set; }
        public string AccountTitle { get; set; }
        public string AccountTypeName { get; set; }
        public int RawNetChange { get; set; }
        public int FractionalUnitFactor { get; set; }
    }
}
