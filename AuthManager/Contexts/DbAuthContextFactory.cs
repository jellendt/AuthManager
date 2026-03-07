using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthManager.Contexts
{
    //needed for generating ef core migrations
    public class DbAuthContextFactory : IDesignTimeDbContextFactory<DbAuthContext>
    {
        public DbAuthContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<DbAuthContext> optionsBuilder = new();

            optionsBuilder.UseSqlServer("Server=...");

            return new DbAuthContext(optionsBuilder.Options);
        }
    }
}
