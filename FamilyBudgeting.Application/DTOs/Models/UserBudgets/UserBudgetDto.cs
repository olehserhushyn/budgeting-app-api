namespace FamilyBudgeting.Domain.DTOs.Models.UserBudgets
{
    public class UserBudgetDto
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public string RoleTitle { get; set; }
        public Guid BudgetId { get; private set; }
    }
}
