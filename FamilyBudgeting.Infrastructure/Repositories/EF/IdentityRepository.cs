using FamilyBudgeting.Domain.Data.Users;
using FamilyBudgeting.Domain.Exceptions;
using FamilyBudgeting.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudgeting.Infrastructure.Repositories.EF
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public IdentityRepository(UserManager<ApplicationUser> userManager, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _userManager = userManager;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ApplicationUser> RegisterAsync(string firstName, string lastName, string email, string password)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser(email, firstName, lastName, false);
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    throw new DomainValidationException(string.Join(" ", result.Errors.Select(e => e.Description)));
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    throw new DomainValidationException(string.Join(" ", roleResult.Errors.Select(e => e.Description)));
                }

                await transaction.CommitAsync();
                return user;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new DomainValidationException($"Registration failed: {ex.Message}");
            }
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
