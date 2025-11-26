namespace FamilyBudgeting.Domain.DTOs.Models.Currencies
{
    public class CurrencyDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Code { get; set; }  // ISO 4217 currency code (e.g., USD, EUR)
        public string Name { get; set; }  // Full currency name (e.g., US Dollar)
        public string? Symbol { get; set; }  // Currency symbol (e.g., $, €, £)
        public int FractionalUnitFactor { get; set; } // Nullable Exchange Rate
        public bool IsActive { get; set; } // Nullable Exchange Rate
    }
}
