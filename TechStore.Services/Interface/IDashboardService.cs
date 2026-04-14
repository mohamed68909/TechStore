namespace TechStore.Services.Interfaces
{
    public interface IDashboardService
    {
        int GetTotalOrdersCount();
        int GetApprovedOrdersCount();
        int GetTotalUsersCount();
        int GetTotalProductsCount();
    }
}
