using DebtManagerApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace DebtManagerApp
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            // This now correctly uses the AppDataHelper to find the database file,
            // ensuring that the design-time tools (like Update-Database) look
            // in the exact same location as the running application.
            var dbPath = AppDataHelper.GetLocalDatabasePath();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}


