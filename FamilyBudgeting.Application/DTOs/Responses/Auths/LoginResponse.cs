namespace FamilyBudgeting.Domain.DTOs.Responses.Auths
{
    public class LoginResponse
    {
        public bool LoginSuccess { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public UserAuthResponse? User { get; set; }
    }
}
