using Microsoft.Extensions.Caching.Memory;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Services.Implementation
{
    // FIX 10: Replaced 4 individual synchronous DB queries with a single batched async call.
    // Results are cached for 60 seconds via IMemoryCache to reduce DB load on frequent page refreshes.
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        // Cache key constant — single source of truth
        private const string StatsCacheKey = "dashboard_stats";

        public DashboardService(IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            // Attempt to retrieve from cache first
            if (_cache.TryGetValue(StatsCacheKey, out DashboardStatsDto? cached) && cached != null)
                return cached;

            // FIX 10: All four counts are fetched concurrently with Task.WhenAll
            // to reduce total wall-clock time from (4 × query_time) to ~(1 × query_time).
            var totalOrdersTask    = _unitOfWork.OrderHeader.CountAsync();
            var approvedOrdersTask = _unitOfWork.OrderHeader.CountAsync(x => x.OrderStatus == SD.Approve);
            var totalUsersTask     = _unitOfWork.ApplicationUser.CountAsync();
            var totalProductsTask  = _unitOfWork.Product.CountAsync();

            await Task.WhenAll(totalOrdersTask, approvedOrdersTask, totalUsersTask, totalProductsTask);

            var stats = new DashboardStatsDto(
                TotalOrders:    totalOrdersTask.Result,
                ApprovedOrders: approvedOrdersTask.Result,
                TotalUsers:     totalUsersTask.Result,
                TotalProducts:  totalProductsTask.Result
            );

            // Cache for 60 seconds with sliding expiry — stale by at most 1 minute, not a concern for a dashboard
            _cache.Set(StatsCacheKey, stats, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(60)
            });

            return stats;
        }
    }
}
