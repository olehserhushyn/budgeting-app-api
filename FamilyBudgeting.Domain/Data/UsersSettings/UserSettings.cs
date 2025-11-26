using FamilyBudgeting.Domain.Core;

namespace FamilyBudgeting.Domain.Data.UsersSettings
{
    public class UserSettings : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid MainCurrencyId { get; private set; }
        public bool ShowOnboarding { get; private set; }

        public UserSettings(Guid userId, Guid mainCurrencyId, bool showOnboarding)
        {
            UserId = userId;
            MainCurrencyId = mainCurrencyId;
            ShowOnboarding = showOnboarding;
        }



        public void Update(Guid mainCurrencyId, bool showOnboarding)
        {
            this.MainCurrencyId = mainCurrencyId;
            this.ShowOnboarding = showOnboarding;

            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}
