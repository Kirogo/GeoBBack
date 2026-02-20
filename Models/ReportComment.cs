using System.ComponentModel.DataAnnotations;

namespace geoback.Models
{
    public class ReportComment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int SiteVisitReportId { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string UserRole { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string CommentText { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation
        public SiteVisitReport SiteVisitReport { get; set; }
        public ReportComment ParentComment { get; set; }
        public ICollection<ReportComment> Replies { get; set; }
    }
}