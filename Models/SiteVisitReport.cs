using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class SiteVisitReport
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RMUserId { get; set; }
        
        [Required]
        public DateTime VisitDate { get; set; }
        
        // Location from geo-tagging
        [Column(TypeName = "decimal(10,8)")]
        public decimal? VisitLatitude { get; set; }
        
        [Column(TypeName = "decimal(11,8)")]
        public decimal? VisitLongitude { get; set; }
        
        public string LocationAddress { get; set; }
        
        // Call report fields from your Word document
        [StringLength(200)]
        public string PersonMet { get; set; }
        
        [StringLength(100)]
        public string PersonDesignation { get; set; }
        
        // Amounts
        [Column(TypeName = "decimal(18,2)")]
        public decimal? BQAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ConstructionLoanAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomerContribution { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DrawnFundsToDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UndrawnFunds { get; set; }
        
        // Site details
        [StringLength(100)]
        public string PlotLRNumber { get; set; }
        
        public string ExactLocation { get; set; }
        
        [StringLength(50)]
        public string SitePin { get; set; } // GPS coordinates as string
        
        // Project Information
        public string CustomerProfile { get; set; }
        
        public string SiteVisitObjectives { get; set; }
        
        public string CompletedWorks { get; set; } // JSON or text
        
        public string OngoingWorks { get; set; } // JSON or text
        
        public string MaterialsOnSite { get; set; } // JSON or text
        
        public string DefectsNoted { get; set; } // JSON or text
        
        // Drawdown request details
        [StringLength(20)]
        public string DrawdownRequestNumber { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RequestedAmount { get; set; }
        
        // Documents submitted flags
        public bool HasQSValuation { get; set; }
        public bool HasInterimCertificate { get; set; }
        public bool HasCustomerInstruction { get; set; }
        public bool HasContractorProgressReport { get; set; }
        public bool HasContractorInvoice { get; set; }
        
        // Workflow state
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, UnderReview, ReturnedToRM, Approved, Rejected
        
        // QS task locking
        [StringLength(100)]
        public string CurrentQSLockedBy { get; set; }
        
        public DateTime? LockedUntil { get; set; }
        
        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        [StringLength(100)]
        public string UpdatedBy { get; set; }
        
        public DateTime? SubmittedAt { get; set; }
        
        public DateTime? ApprovedAt { get; set; }
        
        [StringLength(100)]
        public string ApprovedBy { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedAmount { get; set; }
        
        // Additional fields from Report.cs that are useful
        [StringLength(200)]
        public string? ProjectName { get; set; }
        
        [StringLength(100)]
        public string? LoanType { get; set; }
        
        [StringLength(50)]
        public string? IbpsNo { get; set; } // Keeping for backward compatibility
        
        [StringLength(50)]
        public string? Weather { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public double? Temperature { get; set; }
        
        public string? DocumentsJson { get; set; } // Stores structured document checklist as JSON
        
        // Navigation properties
        [ForeignKey("FacilityId")]
        public Facility Facility { get; set; }
        
        public ICollection<Attachment> Attachments { get; set; }
        
        public ICollection<ApprovalTrailEntry> ApprovalTrail { get; set; }
        
        public ICollection<ReportComment> Comments { get; set; }
        
        // New collections from Report.cs
        public ICollection<WorkProgress> WorkProgress { get; set; }
        
        public ICollection<Issue> Issues { get; set; }
        
        public ICollection<ReportPhoto> ReportPhotos { get; set; }
    }
}