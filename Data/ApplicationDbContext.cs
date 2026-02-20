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
        
        // NEW: Add these for authentication
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Checklist> Checklists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Client configuration
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.CustomerNumber)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.CustomerId)
                .IsUnique();

            // Facility configuration
            modelBuilder.Entity<Facility>()
                .HasIndex(f => f.IBPSNumber)
                .IsUnique();

            modelBuilder.Entity<Checklist>()
                .HasIndex(c => c.DclNo)
                .IsUnique();

            // SiteVisitReport relationships
            modelBuilder.Entity<SiteVisitReport>()
                .HasOne(s => s.Facility)
                .WithMany(f => f.SiteVisitReports)
                .HasForeignKey(s => s.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attachment relationships
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.SiteVisitReport)
                .WithMany(s => s.Attachments)
                .HasForeignKey(a => a.SiteVisitReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApprovalTrail relationships
            modelBuilder.Entity<ApprovalTrailEntry>()
                .HasOne(a => a.SiteVisitReport)
                .WithMany(s => s.ApprovalTrail)
                .HasForeignKey(a => a.SiteVisitReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment relationships
            modelBuilder.Entity<ReportComment>()
                .HasOne(c => c.SiteVisitReport)
                .WithMany(s => s.Comments)
                .HasForeignKey(c => c.SiteVisitReportId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReportComment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkProgress relationships
            modelBuilder.Entity<WorkProgress>()
                .HasOne(w => w.SiteVisitReport)
                .WithMany(s => s.WorkProgress)
                .HasForeignKey(w => w.SiteVisitReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Issue relationships
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.SiteVisitReport)
                .WithMany(s => s.Issues)
                .HasForeignKey(i => i.SiteVisitReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // ReportPhoto relationships
            modelBuilder.Entity<ReportPhoto>()
                .HasOne(p => p.SiteVisitReport)
                .WithMany(s => s.ReportPhotos)
                .HasForeignKey(p => p.SiteVisitReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<SiteVisitReport>()
                .HasIndex(s => s.Status);

            modelBuilder.Entity<SiteVisitReport>()
                .HasIndex(s => s.CurrentQSLockedBy);

            modelBuilder.Entity<ApprovalTrailEntry>()
                .HasIndex(a => a.Timestamp);
        }
    }
}