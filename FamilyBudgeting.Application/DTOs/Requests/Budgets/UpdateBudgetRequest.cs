namespace FamilyBudgeting.Domain.DTOs.Requests.Budgets
{
    public record UpdateBudgetRequest(Guid LedgerId, DateTime StartDate, DateTime EndDate, string Title);
} 