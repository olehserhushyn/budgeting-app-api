using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Categories;
using FamilyBudgeting.Domain.DTOs.Responses.Categories;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class CategoryQueryService : ICategoryQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryQueryService> _logger;

        public CategoryQueryService(IUnitOfWork unitOfWork, ILogger<CategoryQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(Guid ledgerId)
        {
            string query = @"
                SELECT DISTINCT
                    c.""Id"",
                    c.""Title"" AS Title,
                    c.""LedgerId"" as LedgerId,
                    c.""TransactionTypeId"" as TransactionTypeId
                FROM
                    ""Category"" AS c
                WHERE
                    c.""LedgerId"" = @LedgerId
                    AND c.""IsDeleted"" = false
                ORDER BY c.""Title""
            ";

            var qparams = new { LedgerId = ledgerId };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryAsync<CategoryDto>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<GetCategoriesResponse>> GetCategoriesDetailsAsync(Guid ledgerId)
        {
            string query = @"
                SELECT DISTINCT
                    c.""Id"",
                    c.""Title"" AS Title,
                    c.""TransactionTypeId"" as TransactionTypeId,
                    tt.""Title"" as TransactionTypeTitle
                FROM
                    ""Category"" AS c
                INNER JOIN ""TransactionType"" AS tt ON tt.""Id"" = c.""TransactionTypeId""
                WHERE
                    c.""LedgerId"" = @LedgerId
                    AND c.""IsDeleted"" = false
                ORDER BY c.""Title""
            ";

            var qparams = new { LedgerId = ledgerId };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryAsync<GetCategoriesResponse>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<CategoryDto?> GetCategoryAsync(Guid id)
        {
            string query = @"
                SELECT DISTINCT
                    c.""Id"",
                    c.""Title"" AS Title,
                    c.""LedgerId"" as LedgerId,
                    c.""TransactionTypeId"" as TransactionTypeId
                FROM
                    ""Category"" AS c
                WHERE
                    c.""Id"" = @Id
                    AND c.""IsDeleted"" = false
            ";

            var qparams = new { Id = id };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CategoryDto>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<CategoryDto?> GetCategoryAsync(string title)
        {
            string query = @"
                SELECT DISTINCT
                    c.""Id"",
                    c.""Title"" AS Title,
                    c.""LedgerId"" as LedgerId,
                    c.""TransactionTypeId"" as TransactionTypeId
                FROM
                    ""Category"" AS c
                WHERE
                    c.""Title"" ILIKE @Title
                    AND c.""IsDeleted"" = false
            ";

            var qparams = new { Title = title };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CategoryDto>(query, qparams, _unitOfWork.Transaction);
        }
    }
}