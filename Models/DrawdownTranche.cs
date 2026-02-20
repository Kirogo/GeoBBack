using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class DrawdownTranche
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string TrancheNumber { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime RequestDate { get; set; }
        
        public DateTime? DisbursementDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        
        // Navigation
        public Facility Facility { get; set; }
    }
}