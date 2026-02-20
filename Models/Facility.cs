using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class Facility
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IBPSNumber { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalApprovedAmount { get; set; }
        
        [StringLength(500)]
        public string ProjectDescription { get; set; }
        
        [Column(TypeName = "decimal(10,8)")]
        public decimal? SiteLatitude { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? SiteLongitude { get; set; }
        
        public int? AllowedGeoFenceRadius { get; set; } = 100;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(100)]
        public string UpdatedBy { get; set; }
        
        // Navigation properties
        public ICollection<Milestone> Milestones { get; set; }
        public ICollection<DrawdownTranche> Tranches { get; set; }
        public ICollection<SiteVisitReport> SiteVisitReports { get; set; }
    }
}