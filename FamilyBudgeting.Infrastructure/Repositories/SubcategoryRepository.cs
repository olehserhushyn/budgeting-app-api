using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Subcategories;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class SubcategoryRepository : ISubcategoryRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubcategoryRepository> _logger;

        public SubcategoryRepository(IUnitOfWork unitOfWork, ILogger<SubcategoryRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateSubcategoryAsync(Subcategory subcategory)
        {
            string query = @"
                INSERT INTO ""Subcategory""
                (""CategoryId"", ""Title"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                (@CategoryId, @Title, @CreatedAt, @UpdatedAt)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                CategoryId = subcategory.CategoryId,
                Title = subcategory.Title,
                CreatedAt = subcategory.CreatedAt,
                UpdatedAt = subcategory.UpdatedAt
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateSubcategoryAsync(Guid subId, Subcategory subcategory)
        {
            string query = @"
                UPDATE ""Subcategory""
                SET ""CategoryId"" = @CategoryId, 
                    ""Title"" = @Title,
                    ""UpdatedAt"" = @UpdatedAt
                WHERE ""Id"" = @Id;
                ";

            var qparams = new
            {
                Id = subId,
                CategoryId = subcategory.CategoryId,
                Title = subcategory.Title,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogQuery(query, qparams);
            return await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction) > 0;
        }

        public async Task<bool> DeleteSubcategoryAsync(Guid subId)
        {
            string query = @"
                UPDATE ""Subcategory""
                SET ""IsDeleted"" = true,
                    ""UpdatedAt"" = @UpdatedAt
                WHERE ""Id"" = @Id;
                ";

            var parameters = new
            {
                Id = subId,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogQuery(query, parameters);
            return await _unitOfWork.Connection.ExecuteAsync(query, parameters, _unitOfWork.Transaction) > 0;
        }

        public async Task<bool> DeleteSubcategoriesAsync(IEnumerable<Guid> subIds)
        {
            string query = @"
                UPDATE ""Subcategory""
                SET ""IsDeleted"" = true,
                    ""UpdatedAt"" = @UpdatedAt
                WHERE ""Id"" = ANY(@Ids);
                ";

            var parameters = new
            {
                Ids = subIds.ToArray(),
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogQuery(query, parameters);
            return await _unitOfWork.Connection.ExecuteAsync(query, parameters, _unitOfWork.Transaction) > 0;
        }
    }
}