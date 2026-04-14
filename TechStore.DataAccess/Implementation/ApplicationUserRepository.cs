

using TechStore.DataAccess.Data;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;

namespace TechStore.DataAccess.Implementation
{
    public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


    }
}
