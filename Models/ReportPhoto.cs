using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class ReportPhoto
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(500)]
        public string Url { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }
        
        [StringLength(500)]
        public string? Caption { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(100)]
        public string UploadedBy { get; set; } = string.Empty;
        
        public bool IsPrimary { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        // Foreign key to SiteVisitReport
        [Required]
        public int SiteVisitReportId { get; set; }
        
        [ForeignKey("SiteVisitReportId")]
        public SiteVisitReport? SiteVisitReport { get; set; }
        
        // Geo-tagging fields
        [Column(TypeName = "decimal(10,8)")]
        public decimal? PhotoLatitude { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? PhotoLongitude { get; set; }
        
        public bool IsGeoTagged { get; set; }
        
        public DateTime? PhotoTimestamp { get; set; }
        
        // File metadata
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ContentType { get; set; }
        
        public long FileSizeInBytes { get; set; }
    }
}