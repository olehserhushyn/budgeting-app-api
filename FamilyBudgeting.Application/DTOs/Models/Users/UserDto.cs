using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Models.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsDeleted { get; set; }
    }
}
