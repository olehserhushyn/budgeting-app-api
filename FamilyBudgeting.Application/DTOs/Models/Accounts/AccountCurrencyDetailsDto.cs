using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyBudgeting.Domain.DTOs.Models.Accounts
{
    public class AccountCurrencyDetailsDto
    {
        public Guid AccountId { get; set; }
        public DateTime AccountCreatedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountTypeId { get; set; }
        public string AccountTitle { get; set; }
        public string AccountTypeName { get; set; }
        public int AccountBalance { get; set; }
        // currencyS
        public Guid CurrencyId { get; set; }
        public string CurrencyCode { get; set; }  // ISO 4217 currency code (e.g., USD, EUR)
        public string CurrencyName { get; set; }  // Full currency name (e.g., US Dollar)
        public string? CurrencySymbol { get; set; }  // Currency symbol (e.g., $, €, £)
        public int CurrencyFractionalUnitFactor { get; set; } // Nullable Exchange Rate

        [NotMapped]
        public double FormattedAmount => (double)this.AccountBalance / this.CurrencyFractionalUnitFactor;
    }
}
