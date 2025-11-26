using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.Data.UserLedgerRoles
{
    public class UserLedgerRole
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }

        public UserLedgerRole(Guid id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
