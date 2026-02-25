// DTOs/QsDtos.cs
using System.Text.Json.Serialization;

namespace geoback.DTOs
{
    public class QsDashboardStatsDto
    {
        public int PendingReviews { get; set; }
        public int InProgress { get; set; }
        public int CompletedToday { get; set; }
        public int ScheduledVisits { get; set; }
        public string AverageResponseTime { get; set; } = "0h";
        public int CriticalIssues { get; set; }
        public int MyActiveReviews { get; set; }
        public int OverdueReviews { get; set; }
    }

    public class SiteVisitDto
    {
        public Guid Id { get; set; }
        public Guid? ReportId { get; set; }
        public string? ReportTitle { get; set; }
        public string? ProjectName { get; set; }
        public string SiteAddress { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string? ScheduledTime { get; set; }
        public string Status { get; set; } = "scheduled";
        public string? Notes { get; set; }
        public string? RMName { get; set; }
        public string? QSName { get; set; }
    }
}