namespace FamilyBudgeting.Application.DTOs.Requests.Analytics
{
    public record GetAccountSummaryRequest(Guid LedgerId, int Year);
}
