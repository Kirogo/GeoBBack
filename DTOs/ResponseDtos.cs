namespace geoback.DTOs
{
    public class SiteVisitReportResponseDto
    {
        public int Id { get; set; }
        public string? IBPSNumber { get; set; }
        public string? CustomerName { get; set; }
        public DateTime VisitDate { get; set; }
        public string? RMUserId { get; set; }
        public string? Status { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? CurrentQSLockedBy { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public List<AttachmentInfoDto> Attachments { get; set; } = new();
        public List<ApprovalTrailEntryDto> ApprovalTrail { get; set; } = new();
        public List<ReportCommentDto> Comments { get; set; } = new();
    }

    public class AttachmentInfoDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSizeInBytes { get; set; }
        public string AttachmentType { get; set; } = string.Empty;
        public bool IsGeoTagged { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class ApprovalTrailEntryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public DateTime Timestamp { get; set; }
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
    }
}