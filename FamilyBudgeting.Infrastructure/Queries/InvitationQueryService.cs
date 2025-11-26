using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.ValueObjects;
using FamilyBudgeting.Domain.DTOs.Models.Invitations;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class InvitationQueryService : IInvitationQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvitationQueryService> _logger;

        public InvitationQueryService(IUnitOfWork unitOfWork, ILogger<InvitationQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> HasExpireAsync(string token)
        {
            string sql = @"
                UPDATE ""Invitation"" 
                SET 
                    ""Status"" = @Status,
                    ""UpdatedAt"" = CURRENT_TIMESTAMP
                WHERE ""Token"" = @Token 
                AND ""IsDeleted"" = FALSE
                RETURNING ""Id"";";

            var qparams = new
            {
                Status = InvitationStatus.Expired,
                Token = token,
            };

            _logger.LogQuery(sql, qparams);

            var result = await _unitOfWork.Connection.ExecuteScalarAsync<Guid?>(sql, qparams, _unitOfWork.Transaction);
            return result.HasValue;
        }

        public async Task<InvitationDto?> GetByTokenAsync(string token)
        {
            string sql = @"
                SELECT 
                    ""Id"", 
                    ""InvitedEmail"", 
                    ""InvitedRoleId"", 
                    ""InviterUserId"", 
                    ""DestinationType"", 
                    ""DestinationId"", 
                    ""Status"", 
                    ""Token"", 
                    ""ExpiresAt"",
                    ""CreatedAt""
                FROM ""Invitation"" 
                WHERE ""Token"" = @Token 
                AND ""IsDeleted"" = FALSE
                LIMIT 1;";

            var qparams = new { Token = token };

            _logger.LogQuery(sql, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<InvitationDto>(
                sql, qparams, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<InvitationDto>> GetPendingByEmailAsync(string email)
        {
            string sql = @"
                SELECT 
                    ""Id"", 
                    ""InvitedEmail"", 
                    ""InvitedRoleId"", 
                    ""InviterUserId"", 
                    ""DestinationType"", 
                    ""DestinationId"", 
                    ""Status"", 
                    ""Token"", 
                    ""ExpiresAt"",
                    ""CreatedAt""
                FROM ""Invitation"" 
                WHERE ""InvitedEmail"" = @Email 
                AND ""Status"" = @Status::""InvitationStatus""
                AND ""IsDeleted"" = FALSE;";

            var qparams = new { Email = email, Status = InvitationStatus.Pending.ToString() };

            _logger.LogQuery(sql, qparams);

            return await _unitOfWork.Connection.QueryAsync<InvitationDto>(
                sql, qparams, _unitOfWork.Transaction);
        }
    }
}
