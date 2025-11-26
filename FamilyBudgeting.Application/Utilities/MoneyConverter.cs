namespace FamilyBudgeting.Domain.Utilities
{
    public static class MoneyConverter
    {
        /// <summary>
        /// Converts a currency amount (e.g., dollars) to cents (or smallest currency unit) based on the account's currency.
        /// </summary>
        /// <param name="amount">The amount in the currency's main unit (e.g., 11.5 USD).</param>
        /// <param name="currencyFractionalUnitFactor">The Currency Fractional Unit Factor property.</param>
        /// <returns>The amount in cents (or smallest unit) as an int.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the account currency is null or has an invalid fractional unit factor.</exception>
        /// <exception cref="OverflowException">Thrown if the calculated amount exceeds int limits.</exception>
        public static int ConvertToCents(decimal amount,int currencyFractionalUnitFactor)
        {
            if (currencyFractionalUnitFactor <= 0)
            {
                throw new InvalidOperationException("Invalid currency fractional unit factor.");
            }
            decimal intermediateAmount = amount * currencyFractionalUnitFactor;
            if (intermediateAmount > int.MaxValue || intermediateAmount < int.MinValue)
            {
                throw new OverflowException("Calculated amount in cents exceeds integer limits.");
            }
            return (int)Math.Round(intermediateAmount, 0, MidpointRounding.AwayFromZero);
        }

        public static int ConvertToCents(double amount, int currencyFractionalUnitFactor)
        {
            if (currencyFractionalUnitFactor <= 0)
            {
                throw new InvalidOperationException("Invalid currency fractional unit factor.");
            }
            double intermediateAmount = amount * currencyFractionalUnitFactor;
            if (intermediateAmount > int.MaxValue || intermediateAmount < int.MinValue)
            {
                throw new OverflowException("Calculated amount in cents exceeds integer limits.");
            }
            return (int)Math.Round(intermediateAmount, 0, MidpointRounding.AwayFromZero);
        }
    }
}
