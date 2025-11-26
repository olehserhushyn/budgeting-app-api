using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Subcategories;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class SubcategoryQueryService : ISubcategoryQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubcategoryQueryService> _logger;

        public SubcategoryQueryService(IUnitOfWork unitOfWork, ILogger<SubcategoryQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Guid>> GetSubcategoryIdsFromCategoryAsync(Guid categoryId)
        {
            string query = @"
                SELECT ""Id""
                FROM ""Subcategory""
                WHERE ""CategoryId"" = @CategoryId
                AND ""IsDeleted"" = false
                ORDER BY ""Id""
                ";
            _logger.LogQuery(query, new { CategoryId = categoryId });

            return await _unitOfWork.Connection.QueryAsync<Guid>(query, new { CategoryId = categoryId }, _unitOfWork.Transaction);
        }

        public async Task<SubcategoryDto?> GetSubcategoryAsync(Guid id)
        {
            string query = @"
                SELECT ""Id"", ""Title"", ""CategoryId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"" 
                FROM ""Subcategory"" 
                WHERE ""Id"" = @Id
                AND ""IsDeleted"" = false";

            _logger.LogQuery(query, new { Id = id });
            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<SubcategoryDto>(query, new { Id = id }, _unitOfWork.Transaction);
        }
    }
}