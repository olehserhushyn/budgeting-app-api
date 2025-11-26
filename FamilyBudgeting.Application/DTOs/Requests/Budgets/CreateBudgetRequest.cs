namespace FamilyBudgeting.Domain.DTOs.Requests.Budgets
{
    public record CreateBudgetRequest(Guid LedgerId, DateTime StartDate, DateTime EndDate, string? Title);
}
