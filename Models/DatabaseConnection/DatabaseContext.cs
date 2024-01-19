using Microsoft.EntityFrameworkCore;
using Task_Management.Models.Templete;

namespace Task_Management.Models.DatabaseConnection
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> opts) : base(opts)
        {

        }

    }
}
