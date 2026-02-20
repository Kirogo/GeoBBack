using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class WorkProgress
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Range(0, 100)]
        public double Percentage { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        // Foreign key to SiteVisitReport
        [Required]
        public int SiteVisitReportId { get; set; }
        
        [ForeignKey("SiteVisitReportId")]
        public SiteVisitReport? SiteVisitReport { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }
}