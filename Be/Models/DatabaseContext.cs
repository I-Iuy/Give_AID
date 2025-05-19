using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Be.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ánh xạ tên bảng
            modelBuilder.Entity<Account>().ToTable("ACCOUNT");
            modelBuilder.Entity<ContentPage>().ToTable("CONTENT_PAGE");

            // Thiết lập quan hệ 1 Account : N ContentPages
            modelBuilder.Entity<ContentPage>()
                .HasOne(cp => cp.Account)
                .WithMany(a => a.ContentPages)
                .HasForeignKey(cp => cp.AccountId);
        }
    }
}
