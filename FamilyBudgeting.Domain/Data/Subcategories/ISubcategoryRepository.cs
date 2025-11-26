namespace FamilyBudgeting.Domain.Data.Subcategories
{
    public interface ISubcategoryRepository
    {
        Task<Guid> CreateSubcategoryAsync(Subcategory subcategory);
        Task<bool> UpdateSubcategoryAsync(Guid subId, Subcategory subcategory);
        Task<bool> DeleteSubcategoryAsync(Guid subId);
        Task<bool> DeleteSubcategoriesAsync(IEnumerable<Guid> subIds);
    }
}
