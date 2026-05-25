using System.Threading.Tasks;

namespace TechStore.DataAccess.DbInitializer
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}
