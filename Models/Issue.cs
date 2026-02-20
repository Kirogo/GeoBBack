using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class Issue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Severity { get; set; } = "Low";
        
        [StringLength(50)]
        public string Status { get; set; } = "Open";
        
        public DateTime? ResolvedAt { get; set; }
        
        [StringLength(100)]
        public string? ResolvedBy { get; set; }
        
        [StringLength(500)]
        public string? ResolutionNotes { get; set; }
        
        // Foreign key to SiteVisitReport
        [Required]
        public int SiteVisitReportId { get; set; }
        
        [ForeignKey("SiteVisitReportId")]
        public SiteVisitReport? SiteVisitReport { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }
}