using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Users;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class UserQueryService : IUserQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserQueryService> _logger;

        public UserQueryService(IUnitOfWork unitOfWork, ILogger<UserQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            string query = @"
                SELECT ""Id"", ""FirstName"", ""LastName"", ""Email"", ""PasswordHash"" 
                FROM ""User""
                WHERE ""Email"" = @Email
                AND ""IsDeleted"" = false
                ORDER BY ""Id""
                LIMIT 1
                ";

            _logger.LogQuery(query, new { Email = email });

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserDto?>(query,
                new
                {
                    Email = email
                });
        }
    }
}