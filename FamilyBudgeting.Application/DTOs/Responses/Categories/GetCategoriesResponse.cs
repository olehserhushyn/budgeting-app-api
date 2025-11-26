namespace FamilyBudgeting.Domain.DTOs.Responses.Categories
{
    public class GetCategoriesResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid TransactionTypeId { get; set; }
        public string TransactionTypeTitle { get; set; }
    }
}
