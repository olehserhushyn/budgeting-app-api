namespace FamilyBudgeting.Application.DTOs.Models.Analytics
{
    public class AccountBalanceDataRawDto
    {
        public Guid AccountId { get; set; }
        public string AccountTitle { get; set; }
        public int RawBalance { get; set; }
        public string AccountTypeName { get; set; }
        public int FractionalUnitFactor { get; set; }
    }
}
