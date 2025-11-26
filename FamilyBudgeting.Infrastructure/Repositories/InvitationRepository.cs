using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Invitations;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InvitationRepository> _logger;

        public InvitationRepository(IUnitOfWork unitOfWork, ILogger<InvitationRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateAsync(Invitation invitation)
        {
            string sql = @"
                INSERT INTO ""Invitation"" 
                (
                    ""InvitedEmail"",
                    ""InvitedRoleId"",
                    ""InviterUserId"",
                    ""DestinationType"",
                    ""DestinationId"",
                    ""Status"",
                    ""Token"",
                    ""ExpiresAt""
                )
                VALUES 
                (
                    @InvitedEmail,
                    @InvitedRoleId,
                    @InviterUserId,
                    @DestinationType,
                    @DestinationId,
                    @Status,
                    @Token,
                    @ExpiresAt
                )
                RETURNING ""Id""; ";

            var qParams = new
            {
                InvitedEmail = invitation.InvitedEmail,
                InvitedRoleId = invitation.InvitedRoleId,
                InviterUserId = invitation.InviterUserId,
                DestinationType = invitation.DestinationType,
                DestinationId = invitation.DestinationId,
                Status = invitation.Status,
                Token = invitation.Token,
                ExpiresAt = invitation.ExpiresAt
            };

            _logger.LogQuery(sql, invitation);
            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(
                sql, qParams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateAsync(Invitation invitation)
        {
            string sql = @"
                UPDATE ""Invitation"" 
                SET 
                    ""Status"" = @Status,
                    ""UpdatedAt"" = @UpdatedAt
                WHERE ""Token"" = @Token 
                AND ""IsDeleted"" = FALSE
                RETURNING ""Id"";";

            var qparams = new
            {
                Status = invitation.Status,
                Token = invitation.Token,
                UpdatedAt = invitation.UpdatedAt
            };

            _logger.LogQuery(sql, qparams);

            var result = await _unitOfWork.Connection.ExecuteScalarAsync<Guid?>(sql, qparams, _unitOfWork.Transaction);
            return result.HasValue;
        }

        
    }
}