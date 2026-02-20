namespace geoback.DTOs
{
    public class CreateSiteVisitReportDto
    {
        public string IBPSNumber { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        
        // Location
        public decimal? VisitLatitude { get; set; }
        public decimal? VisitLongitude { get; set; }
        public string? LocationAddress { get; set; }
        
        // Call report fields
        public string? PersonMet { get; set; }
        public string? PersonDesignation { get; set; }
        
        // Amounts
        public decimal? BQAmount { get; set; }
        public decimal? ConstructionLoanAmount { get; set; }
        public decimal? CustomerContribution { get; set; }
        public decimal? DrawnFundsToDate { get; set; }
        public decimal? UndrawnFunds { get; set; }
        
        // Site details
        public string? PlotLRNumber { get; set; }
        public string? ExactLocation { get; set; }
        public string? SitePin { get; set; }
        
        // Project Information
        public string? CustomerProfile { get; set; }
        public string? SiteVisitObjectives { get; set; }
        public string? CompletedWorks { get; set; }
        public string? OngoingWorks { get; set; }
        public string? MaterialsOnSite { get; set; }
        public string? DefectsNoted { get; set; }
        
        // Drawdown request
        public string? DrawdownRequestNumber { get; set; }
        public decimal RequestedAmount { get; set; }
        
        // Document flags
        public bool HasQSValuation { get; set; }
        public bool HasInterimCertificate { get; set; }
        public bool HasCustomerInstruction { get; set; }
        public bool HasContractorProgressReport { get; set; }
        public bool HasContractorInvoice { get; set; }
    }

    public class SubmitReportDto
    {
        public int ReportId { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }

    public class QSActionDto
    {
        public int ReportId { get; set; }
        public string Action { get; set; } = string.Empty; // Approve, Reject, Return
        public string? Comments { get; set; }
        public decimal? ApprovedAmount { get; set; } // For approval
    }

    public class ReportCommentDto
    {
        public int ReportId { get; set; }
        public int? ParentCommentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }

    public class LockReportDto
    {
        public int ReportId { get; set; }
        public int? LockDurationMinutes { get; set; } = 120;
    }
}