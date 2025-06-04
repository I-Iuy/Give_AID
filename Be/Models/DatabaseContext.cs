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
        // Migration: Add-Migration Init1
        public DbSet<Partner> Partners { get; set; }
        // Migration: Add-Migration Init2
        public DbSet<Ngo> Ngos { get; set; }
        // Migration: Add-Migration Init3
        //public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignPartner> CampaignPartners { get; set; }
        public DbSet<CampaignNgo> CampaignNgos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define composite keys for many-to-many relationships
            modelBuilder.Entity<CampaignPartner>()
                .HasKey(pp => new { pp.CampaignId, pp.PartnerId });

            modelBuilder.Entity<CampaignNgo>()
                .HasKey(pn => new { pn.CampaignId, pn.NgoId });

            // Configure Comment entity
            modelBuilder.Entity<Comment>()
                .Property(c => c.CommentId)
                .ValueGeneratedOnAdd(); // Configure CommentId as identity

            // Configure Comment relationships
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Campaign)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent circular cascade delete

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Account)
                .WithMany()
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.SetNull); // Set AccountId to null if Account is deleted

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Campaign> Campaigns { get; set; } 
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Share> Shares { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

    }
}
