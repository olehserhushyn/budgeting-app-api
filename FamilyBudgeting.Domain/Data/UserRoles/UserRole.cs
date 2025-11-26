namespace FamilyBudgeting.Domain.Data.UserRoles
{
    public class UserRole
    {
        public UserRole(Guid id, string title)
        {
            Id = id;
            Title = title;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
    }
}
