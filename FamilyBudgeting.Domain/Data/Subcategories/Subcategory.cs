namespace FamilyBudgeting.Domain.Data.Subcategories
{
    public class Subcategory : BaseEntity
    {
        public string Title { get; private set; }
        public Guid CategoryId { get; private set; }

        public Subcategory(string title, Guid categoryId)
        {
            Title = title;
            CategoryId = categoryId;
        }

        public void UpdateDetails(string title, Guid categoryId)
        {
            Title = title;
            CategoryId = categoryId;
        }

        public void Delete()
        {
            this.IsDeleted = true;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}
