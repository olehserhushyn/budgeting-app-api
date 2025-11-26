using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Categories;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(IUnitOfWork unitOfWork, ILogger<CategoryRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateCategoryAsync(Category category)
        {
            string query = @"
                INSERT INTO ""Category""
                (""LedgerId"", ""Title"", ""TransactionTypeId"")
                VALUES
                (@LedgerId, @Title, @TransactionTypeId)
                RETURNING ""Id"";
            ";

            var qparams = new
            {
                LedgerId = category.LedgerId,
                Title = category.Title,
                TransactionTypeId = category.TransactionTypeId
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateCategoryAsync(Guid categoryId, Category category)
        {
            string query = @"
                UPDATE ""Category""
                SET ""Title"" = @Title, 
                    ""IsDeleted"" = @IsDeleted, 
                    ""UpdatedAt"" = @UpdatedAt, 
                    ""TransactionTypeId"" = @TransactionTypeId
                WHERE ""Id"" = @Id;
            ";

            var qparams = new
            {
                Id = categoryId,
                Title = category.Title,
                IsDeleted = category.IsDeleted,
                UpdatedAt = category.UpdatedAt,
                TransactionTypeId = category.TransactionTypeId
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteAsync(
                query,
                qparams,
                _unitOfWork.Transaction
            ) > 0;
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId, IEnumerable<Guid> subIds)
        {
            try
            {
                // Delete subcategories
                string deleteSubcategoriesQuery = @"
                    UPDATE ""Subcategory""
                    SET ""IsDeleted"" = true, 
                        ""UpdatedAt"" = @UpdatedAt
                    WHERE ""Id"" = ANY(@Ids);
                ";

                var subcategoriesResult = await _unitOfWork.Connection.ExecuteAsync(
                    deleteSubcategoriesQuery,
                    new { Ids = subIds.ToArray(), UpdatedAt = DateTime.UtcNow },
                    _unitOfWork.Transaction
                );

                _logger.LogQuery(deleteSubcategoriesQuery, new { Ids = subIds });

                // Delete category
                string deleteCategoryQuery = @"
                    UPDATE ""Category""
                    SET ""IsDeleted"" = true, 
                        ""UpdatedAt"" = @UpdatedAt
                    WHERE ""Id"" = @Id;
                ";

                var categoryResult = await _unitOfWork.Connection.ExecuteAsync(
                    deleteCategoryQuery,
                    new { Id = categoryId, UpdatedAt = DateTime.UtcNow },
                    _unitOfWork.Transaction
                );

                _logger.LogQuery(deleteCategoryQuery, new { Id = categoryId, UpdatedAt = DateTime.UtcNow });

                return categoryResult > 0 && (subIds.Any() ? subcategoriesResult > 0 : true);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                _logger.LogError($"Operation failed: {ex.Message}");
                throw; // Let UnitOfWork handle rollback
            }
        }
    }
}