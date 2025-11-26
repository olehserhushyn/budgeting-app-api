using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Users;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IUnitOfWork unitOfWork, ILogger<UserRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateUserAsync(ApplicationUser user)
        {
            string query = @"
                INSERT INTO ""User"" 
                (""FirstName"", ""LastName"", ""Email"", ""PasswordHash"")
                VALUES 
                (@FirstName, @LastName, LOWER(@Email), @PasswordHash)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PasswordHash = user.PasswordHash
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(
                query,
                qparams,
                _unitOfWork.Transaction
            );
        }
    }
}