namespace geoback.DTOs
{
    public class FacilityDto
    {
        public int Id { get; set; }
        public string IBPSNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalApprovedAmount { get; set; }
        public string? ProjectDescription { get; set; }
        public decimal? SiteLatitude { get; set; }
        public decimal? SiteLongitude { get; set; }
        public int AllowedGeoFenceRadius { get; set; } = 100;
        public List<MilestoneDto> Milestones { get; set; } = new();
        public List<DrawdownTrancheDto> Tranches { get; set; } = new();
    }

    public class MilestoneDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int MilestoneOrder { get; set; }
        public decimal AllocatedAmount { get; set; }
        public bool IsAchieved { get; set; }
        public DateTime? AchievedDate { get; set; }
    }

    public class DrawdownTrancheDto
    {
        public int Id { get; set; }
        public string TrancheNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}