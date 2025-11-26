using Microsoft.AspNetCore.Identity;

namespace FamilyBudgeting.Domain.Data.Users
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public ApplicationUser(string email, string firstName, string lastName, bool isDeleted)
        {
            Id = Guid.NewGuid();  // IdentityUser<Guid> requires an ID
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Email = email.ToLower();
            UserName = email.ToLower();
            FirstName = firstName;
            LastName = lastName;
            IsDeleted = isDeleted;
        }

        public void UpdateEmailConfirmationStatus(bool status)
        {
            EmailConfirmed = status;
        }

        public void UpdateProfile(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
