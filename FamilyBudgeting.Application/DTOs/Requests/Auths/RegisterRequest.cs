using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Requests.Auths
{
    public record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password
    );
}
