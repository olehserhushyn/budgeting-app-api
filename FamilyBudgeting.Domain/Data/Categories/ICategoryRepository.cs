namespace FamilyBudgeting.Domain.Data.Categories
{
    public interface ICategoryRepository
    {
        Task<Guid> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Guid categoryId, Category category);
        Task<bool> DeleteCategoryAsync(Guid categoryId, IEnumerable<Guid> subIds);
    }
}
