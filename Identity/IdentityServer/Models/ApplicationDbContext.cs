using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Models
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> context)
            : base(context) { }
    }
}
