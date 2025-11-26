namespace FamilyBudgeting.Domain.Data.Currencies
{
    public class Currency
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public string Code { get; private set; }  // ISO 4217 currency code (e.g., USD, EUR)
        public string Name { get; private set; }  // Full currency name (e.g., US Dollar)
        public string? Symbol { get; private set; }  // Currency symbol (e.g., $, €, £)
        public int FractionalUnitFactor { get; private set; } // Nullable Exchange Rate
        public bool IsActive { get; private set; } // Nullable Exchange Rate

        public Currency(DateTime createdAt, DateTime updatedAt, string code, 
            string name, string? symbol, int fractionalUnitFactor, bool isActive)
        {
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            Code = code;
            Name = name;
            Symbol = symbol;
            FractionalUnitFactor = fractionalUnitFactor;
            IsActive = isActive;
        }
    }
}
