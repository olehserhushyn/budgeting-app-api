namespace FamilyBudgeting.Domain.Data.UserLedgers
{
    public class UserLedger : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public Guid LedgerId { get; private set; }

        public UserLedger(Guid userId, Guid roleId, Guid ledgerId)
        {
            UserId = userId;
            RoleId = roleId;
            LedgerId = ledgerId;
        }

        public void Update(Guid roleId)
        {
            this.RoleId = roleId;

            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}
