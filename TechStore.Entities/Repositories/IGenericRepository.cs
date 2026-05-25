

using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TechStore.Entities.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        // FIX 11: Removed stale scaffolding comments

        IEnumerable<T> GetAll(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,    // FIX 11: Renamed Includeword → includeWord
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        // FIX 11: Renamed GetFirstorDefault → GetFirstOrDefault (matches .NET conventions)
        // FIX H-3: Return type changed to T? (nullable) to eliminate unsafe null-forgiving !
        T? GetFirstOrDefault(Expression<Func<T, bool>>? predicate = null, string? includeWord = null);

        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, string? includeWord = null);

        IEnumerable<T> GetPaginated(
            int pageNumber, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null); // FIX M-9: Added orderBy

        Task<IEnumerable<T>> GetPaginatedAsync(
            int pageNumber, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        int Count(Expression<Func<T, bool>>? predicate = null);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        void Add(T entity);
        Task AddAsync(T entity);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
