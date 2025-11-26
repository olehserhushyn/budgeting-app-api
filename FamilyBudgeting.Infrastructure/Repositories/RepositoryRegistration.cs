using Microsoft.Extensions.DependencyInjection;
using FamilyBudgeting.Domain.Data.Invitations;
using FamilyBudgeting.Domain.Data.UserBudgets;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public static class RepositoryRegistration
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IUserBudgetRepository, UserBudgetRepository>();
        }
    }
} 