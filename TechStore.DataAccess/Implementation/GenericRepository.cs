using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TechStore.DataAccess.Data;
using TechStore.Entities.Repositories;

namespace TechStore.DataAccess.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet; // FIX 11: made readonly — never reassigned after ctor

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public void Add(T entity) => _dbSet.Add(entity);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        // ── GetAll ──────────────────────────────────────────────────────────
        public IEnumerable<T> GetAll(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,   // FIX 11: renamed from Includeword
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            if (includeWord != null)
                foreach (var nav in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(nav.Trim());
            return orderBy != null ? orderBy(query).ToList() : query.ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            if (includeWord != null)
                foreach (var nav in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(nav.Trim());
            return orderBy != null
                ? await orderBy(query).ToListAsync()
                : await query.ToListAsync();
        }

        // ── GetFirstOrDefault ───────────────────────────────────────────────
        // FIX 11: Renamed GetFirstorDefault → GetFirstOrDefault
        // FIX H-3: Returns T? (nullable) — removed the dangerous null-forgiving ! operator
        public T? GetFirstOrDefault(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            if (includeWord != null)
                foreach (var nav in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(nav.Trim());
            return query.FirstOrDefault(); // nullable — callers must null-check
        }

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            if (includeWord != null)
                foreach (var nav in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(nav.Trim());
            return await query.FirstOrDefaultAsync();
        }

        // ── GetPaginated ────────────────────────────────────────────────────
        // FIX M-9: Added orderBy parameter — without ORDER BY, SQL Server can return
        // rows in any order, causing pages to overlap or miss records.
        public IEnumerable<T> GetPaginated(
            int pageNumber, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            if (includeWord != null)
                foreach (var nav in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(nav.Trim());
            if (orderBy != null) query = orderBy(query);
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<IEnumerable<T>> GetPaginatedAsync(
            int pageNumber, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            string? includeWord = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            if (includeWord != null)
                foreach (var nav in includeWord.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(nav.Trim());
            if (orderBy != null) query = orderBy(query);
            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        // ── Count ───────────────────────────────────────────────────────────
        public int Count(Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            return query.Count();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> query = _dbSet;
            if (predicate != null) query = query.Where(predicate);
            return await query.CountAsync();
        }

        // ── Remove ──────────────────────────────────────────────────────────
        public void Remove(T entity) => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
    }
}
