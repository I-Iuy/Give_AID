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
        // Migration: Add-Migration Init
        public DbSet<Purpose> Purposes { get; set; }
        // Migration: Add-Migration Int1
        public DbSet<Partner> Partners { get; set; }
    }
}
