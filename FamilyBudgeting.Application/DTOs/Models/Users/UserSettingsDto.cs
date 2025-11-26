namespace FamilyBudgeting.Domain.DTOs.Models.Users
{
    public class UserSettingsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid MainCurrencyId { get; set; }
        public bool ShowOnboarding { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
