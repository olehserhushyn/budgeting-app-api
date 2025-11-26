namespace FamilyBudgeting.Domain.Core
{
    public interface IQueryBuilder<T> where T : class
    {
        IQueryBuilder<T> ForUpdate();
        IQueryBuilder<T> ForNoKeyUpdate();
        IQueryBuilder<T> SkipLocked();
        Task<T?> QueryFirstOrDefaultAsync();
        Task<IEnumerable<T>> QueryManyAsync();
    }
}
