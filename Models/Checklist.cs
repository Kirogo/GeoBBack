// Models/Checklist.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class Checklist
    {
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

        // Add this field to store the site visit form data
        public string? SiteVisitFormJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}