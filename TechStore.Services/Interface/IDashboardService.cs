namespace TechStore.Services.Interfaces
{
    // FIX 10: All methods are now async to avoid blocking thread pool threads on DB calls
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }

    /// <summary>DTO carrying all dashboard counters in a single DB query.</summary>
    public record DashboardStatsDto(
        int TotalOrders,
        int ApprovedOrders,
        int TotalUsers,
        int TotalProducts
    );
}
