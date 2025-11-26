using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyBudgeting.Domain.DTOs.Responses.Dashboard
{
    public class DashBoardSummaryResponse
    {
        public DashBoardSummaryResponse_TotalBalance TotalBalance { get; set; }
        public DashBoardSummaryResponse_MonthlyFlow MonthlyIncome { get; set; }
        public DashBoardSummaryResponse_MonthlyFlow MonthlyExpense { get; set; }
        public DashBoardSummaryResponse_Goal Goals { get; set; }
    }

    public class DashBoardSummaryResponse_TotalBalance
    {
        public decimal FormattedTotalAmount { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyTitle { get; set; }
        public int CurrencyFractionalUnitFactor { get; set; }
        public decimal LastMonthPercent { get; set; }
    }

    public class DashBoardSummaryResponse_MonthlyFlow
    {
        public decimal FormattedAmount { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyTitle { get; set; }
        public int CurrencyFractionalUnitFactor { get; set; }
        public decimal LastMonthPercent { get; set; }
    }

    public class DashBoardSummaryResponse_Goal
    {
        public int Total { get; set; }
        public int Done { get; set; }
        public int LastMonthDone { get; set; }
    }
}
