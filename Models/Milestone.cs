using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace geoback.Models
{
    public class Milestone
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; }
        
        public int MilestoneOrder { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AllocatedAmount { get; set; }
        
        public bool IsAchieved { get; set; }
        
        public DateTime? AchievedDate { get; set; }
        
        // Navigation
        public Facility Facility { get; set; }
    }
}