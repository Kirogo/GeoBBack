// Data/ApplicationDbContext.cs (add Comments DbSet if not already there)
using Microsoft.EntityFrameworkCore;
using geoback.Models;

namespace geoback.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Existing DbSets
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<DrawdownTranche> DrawdownTranches { get; set; }
        public DbSet<SiteVisitReport> SiteVisitReports { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<ApprovalTrailEntry> ApprovalTrailEntries { get; set; }
        public DbSet<ReportComment> ReportComments { get; set; }
        public DbSet<WorkProgress> WorkProgress { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<ReportPhoto> ReportPhotos { get; set; }
        
        // Authentication and Core Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        
        // NEW: Comments table for QS reviews
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... rest of your existing configuration ...

            // Comment configuration
            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.ReportId);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasIndex(c => c.CreatedAt);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Report)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}