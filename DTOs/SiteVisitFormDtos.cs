// GeoBack/DTOs/SiteVisitFormDtos.cs
using System.Text.Json.Serialization;

namespace geoback.DTOs
{
    public class SiteVisitFormDto
    {
        public string CallReportNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string SiteVisitDateTime { get; set; } = string.Empty;
        public string PersonMetAtSite { get; set; } = string.Empty;
        public string BqAmount { get; set; } = string.Empty;
        public string ConstructionLoanAmount { get; set; } = string.Empty;
        public string CustomerContribution { get; set; } = string.Empty;
        public string DrawnFundsD1 { get; set; } = string.Empty;
        public string DrawnFundsD2 { get; set; } = string.Empty;
        public string DrawnFundsSubtotal { get; set; } = string.Empty;
        public string UndrawnFundsToDate { get; set; } = string.Empty;
        public string BriefProfile { get; set; } = string.Empty;
        public string SiteExactLocation { get; set; } = string.Empty;
        public string HouseLocatedAlong { get; set; } = string.Empty;
        public string SitePin { get; set; } = string.Empty;
        public string SecurityDetails { get; set; } = string.Empty;
        public string PlotLrNo { get; set; } = string.Empty;
        public string SiteVisitObjective1 { get; set; } = string.Empty;
        public string SiteVisitObjective2 { get; set; } = string.Empty;
        public string SiteVisitObjective3 { get; set; } = string.Empty;
        public string WorksComplete { get; set; } = string.Empty;
        public string WorksOngoing { get; set; } = string.Empty;
        public string MaterialsFoundOnSite { get; set; } = string.Empty;
        public string DefectsNotedOnSite { get; set; } = string.Empty;
        public string DrawdownRequestNo { get; set; } = string.Empty;
        public string DrawdownKesAmount { get; set; } = string.Empty;
        public SiteVisitSubmittedDocsDto DocumentsSubmitted { get; set; } = new();
        public string PreparedBy { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string PreparedDate { get; set; } = string.Empty;
        public List<string> ProgressPhotosPage3 { get; set; } = new List<string> { "", "", "", "" };
        public List<string> ProgressPhotosPage4 { get; set; } = new List<string> { "", "", "", "" };
        public List<string> MaterialsOnSitePhotos { get; set; } = new List<string> { "", "", "", "" };
        public List<string> DefectsNotedPhotos { get; set; } = new List<string> { "", "", "", "" };
    }

    public class SiteVisitSubmittedDocsDto
    {
        public string QsValuation { get; set; } = string.Empty;
        public string InterimCertificate { get; set; } = string.Empty;
        public string CustomerInstructionLetter { get; set; } = string.Empty;
        public string ContractorProgressReport { get; set; } = string.Empty;
        public string ContractorInvoice { get; set; } = string.Empty;
    }
}