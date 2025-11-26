namespace FamilyBudgeting.Domain.DTOs.Responses.Auths
{
    public class UserAuthResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string[] Roles { get; set; } = new string[0];
        public FamilyBudgeting.Domain.DTOs.Models.Users.UserSettingsDto? UserSettings { get; set; }
    }
}
