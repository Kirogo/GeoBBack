// Models/Checklist.cs (update the ICollection<Comment> part)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    [Table("Checklists")]
    public class Checklist
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string DclNo { get; set; } = string.Empty;

        public string? CustomerId { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }

        [Column("LoanType")]
        public string ProjectName { get; set; } = string.Empty;

        public string? IbpsNo { get; set; }
        public Guid? AssignedToRM { get; set; }
        public Guid? CreatedBy { get; set; }
        public string Status { get; set; } = "pending";

        [Required]
        public string DocumentsJson { get; set; } = "[]";

        public string? SiteVisitFormJson { get; set; }

        // Lock fields
        public bool IsLocked { get; set; } = false;
        public Guid? LockedByUserId { get; set; }
        public string? LockedByUserName { get; set; }
        public DateTime? LockedAt { get; set; }

        // QS fields
        public string? AssignedToQS { get; set; }
        public string? AssignedToQSName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? Priority { get; set; } = "Medium";
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties - initialize as empty collection to avoid null reference warnings
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}