using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Be.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Purpose> Purposes { get; set; }
    }
}
