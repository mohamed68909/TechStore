using System.Threading.Tasks;
using TechStore.Entities.Models;

namespace TechStore.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string> roles);
    }
}
