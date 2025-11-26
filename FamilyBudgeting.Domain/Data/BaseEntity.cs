namespace FamilyBudgeting.Domain.Data
{
    public class BaseEntity
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; }

        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public virtual void Delete()
        {
            this.IsDeleted = true;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}
