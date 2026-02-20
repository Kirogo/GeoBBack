using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int SiteVisitReportId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        
        [StringLength(100)]
        public string? ContentType { get; set; }
        
        public long FileSizeInBytes { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AttachmentType { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,8)")]
        public decimal? PhotoLatitude { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? PhotoLongitude { get; set; }
        
        public DateTime? PhotoTimestamp { get; set; }
        
        public bool IsGeoTagged { get; set; }
        
        // Navigation property
        [ForeignKey("SiteVisitReportId")]
        public SiteVisitReport? SiteVisitReport { get; set; }
    }
}