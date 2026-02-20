using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class ApprovalTrailEntry
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int SiteVisitReportId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string UserRole { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Comments { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string? PreviousStatus { get; set; }
        
        [StringLength(50)]
        public string? NewStatus { get; set; }
        
        // Navigation
        [ForeignKey("SiteVisitReportId")]
        public SiteVisitReport? SiteVisitReport { get; set; }
    }
}