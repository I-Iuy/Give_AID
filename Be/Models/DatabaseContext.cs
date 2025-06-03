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
        // Migration: Add-Migration Init1
        public DbSet<Partner> Partners { get; set; }
        // Migration: Add-Migration Init2
        public DbSet<Ngo> Ngos { get; set; }
        // Migration: Add-Migration Init3
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignPartner> CampaignPartners { get; set; }
        public DbSet<CampaignNgo> CampaignNgos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define composite keys for many-to-many relationships
            modelBuilder.Entity<CampaignPartner>()
                .HasKey(pp => new { pp.CampaignId, pp.PartnerId });

            modelBuilder.Entity<CampaignNgo>()
                .HasKey(pn => new { pn.CampaignId, pn.NgoId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
