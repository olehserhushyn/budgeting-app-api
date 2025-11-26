namespace FamilyBudgeting.Domain.DTOs.Models.UserBudgets
{
    public class UserBudgetDetailsDto
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public Guid RoleId { get; private set; }
        public string RoleTitle { get; set; }
        public Guid BudgetId { get; private set; }
    }
}
