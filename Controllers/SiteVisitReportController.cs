using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using geoback.Data;
using geoback.Models;
using geoback.DTOs;
using geoback.Services;

namespace geoback.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteVisitReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFacilityService _facilityService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SiteVisitReportController> _logger;

        public SiteVisitReportController(
            ApplicationDbContext context,
            IFacilityService facilityService,
            INotificationService notificationService,
            ILogger<SiteVisitReportController> logger)
        {
            _context = context;
            _facilityService = facilityService;
            _notificationService = notificationService;
            _logger = logger;
        }

        // RM: Create draft report
        [HttpPost]
        public async Task<ActionResult<SiteVisitReportResponseDto>> CreateReport([FromBody] CreateSiteVisitReportDto dto)
        {
            try
            {
                // Get current user from SSO (you'll need to implement this)
                var currentUserId = User.Identity.Name;
                var currentUserRole = "RM"; // Get from claims

                // Validate IBPS number
                var facility = await _facilityService.GetFacilityByIBPSNumberAsync(dto.IBPSNumber);
                if (facility == null)
                    return BadRequest($"Invalid IBPS number: {dto.IBPSNumber}");

                // Create report
                var report = new SiteVisitReport
                {
                    FacilityId = facility.Id,
                    RMUserId = currentUserId,
                    VisitDate = dto.VisitDate,
                    VisitLatitude = dto.VisitLatitude,
                    VisitLongitude = dto.VisitLongitude,
                    LocationAddress = dto.LocationAddress,
                    PersonMet = dto.PersonMet,
                    PersonDesignation = dto.PersonDesignation,
                    BQAmount = dto.BQAmount,
                    ConstructionLoanAmount = dto.ConstructionLoanAmount,
                    CustomerContribution = dto.CustomerContribution,
                    DrawnFundsToDate = dto.DrawnFundsToDate,
                    UndrawnFunds = dto.UndrawnFunds,
                    PlotLRNumber = dto.PlotLRNumber,
                    ExactLocation = dto.ExactLocation,
                    SitePin = dto.SitePin,
                    CustomerProfile = dto.CustomerProfile,
                    SiteVisitObjectives = dto.SiteVisitObjectives,
                    CompletedWorks = dto.CompletedWorks,
                    OngoingWorks = dto.OngoingWorks,
                    MaterialsOnSite = dto.MaterialsOnSite,
                    DefectsNoted = dto.DefectsNoted,
                    DrawdownRequestNumber = dto.DrawdownRequestNumber,
                    RequestedAmount = dto.RequestedAmount,
                    HasQSValuation = dto.HasQSValuation,
                    HasInterimCertificate = dto.HasInterimCertificate,
                    HasCustomerInstruction = dto.HasCustomerInstruction,
                    HasContractorProgressReport = dto.HasContractorProgressReport,
                    HasContractorInvoice = dto.HasContractorInvoice,
                    Status = "Draft",
                    CreatedBy = currentUserId
                };

                _context.SiteVisitReports.Add(report);
                await _context.SaveChangesAsync();

                // Add to approval trail
                _context.ApprovalTrailEntries.Add(new ApprovalTrailEntry
                {
                    SiteVisitReportId = report.Id,
                    UserId = currentUserId,
                    UserRole = currentUserRole,
                    Action = "Created",
                    NewStatus = "Draft",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return Ok(MapToResponseDto(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating site visit report");
                return StatusCode(500, "An error occurred while creating the report");
            }
        }

        // RM: Submit report with attachments
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitReport(int id, [FromForm] SubmitReportDto dto)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var currentUserRole = "RM";

                var report = await _context.SiteVisitReports
                    .Include(r => r.Facility)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (report == null)
                    return NotFound("Report not found");

                if (report.RMUserId != currentUserId)
                    return Forbid("You can only submit your own reports");

                // Process attachments
                if (dto.Attachments != null && dto.Attachments.Any())
                {
                    foreach (var file in dto.Attachments)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        var attachment = new Attachment
                        {
                            SiteVisitReportId = report.Id,
                            FileName = file.FileName,
                            FileData = memoryStream.ToArray(),
                            ContentType = file.ContentType,
                            FileSizeInBytes = file.Length,
                            AttachmentType = DetermineAttachmentType(file.FileName),
                            IsGeoTagged = false // Will be set by frontend for photos
                        };

                        _context.Attachments.Add(attachment);
                    }
                }

                // Update report status
                report.Status = "Submitted";
                report.SubmittedAt = DateTime.UtcNow;
                report.UpdatedBy = currentUserId;
                report.UpdatedAt = DateTime.UtcNow;

                // Add to approval trail
                _context.ApprovalTrailEntries.Add(new ApprovalTrailEntry
                {
                    SiteVisitReportId = report.Id,
                    UserId = currentUserId,
                    UserRole = currentUserRole,
                    Action = "Submitted",
                    PreviousStatus = "Draft",
                    NewStatus = "Submitted",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Notify all QSs
                await _notificationService.NotifyQSsNewReportAsync(report.Id, report.Facility.IBPSNumber);

                return Ok(new { message = "Report submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report {ReportId}", id);
                return StatusCode(500, "An error occurred while submitting the report");
            }
        }

        // QS: Get all pending reports (pool)
        [HttpGet("pending")]
        public async Task<ActionResult<List<SiteVisitReportResponseDto>>> GetPendingReports()
        {
            try
            {
                var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                if (currentUserRole != "QS")
                    return Forbid("Only QS users can access this endpoint");

                var pendingReports = await _context.SiteVisitReports
                    .Include(r => r.Facility)
                    .Include(r => r.Attachments)
                    .Where(r => r.Status == "Submitted" || r.Status == "UnderReview")
                    .OrderByDescending(r => r.SubmittedAt)
                    .Select(r => MapToResponseDto(r))
                    .ToListAsync();

                return Ok(pendingReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reports");
                return StatusCode(500, "An error occurred while retrieving reports");
            }
        }

        // QS: Lock a report for review
        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockReport(int id, [FromBody] LockReportDto dto)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (currentUserRole != "QS")
                    return Forbid("Only QS users can lock reports");

                var report = await _context.SiteVisitReports.FindAsync(id);
                if (report == null)
                    return NotFound("Report not found");

                // Check if already locked
                if (!string.IsNullOrEmpty(report.CurrentQSLockedBy) && report.LockedUntil > DateTime.UtcNow)
                {
                    return BadRequest($"Report is already locked by {report.CurrentQSLockedBy} until {report.LockedUntil}");
                }

                // Lock the report
                report.CurrentQSLockedBy = currentUserId;
                report.LockedUntil = DateTime.UtcNow.AddMinutes(dto.LockDurationMinutes ?? 120);
                report.UpdatedAt = DateTime.UtcNow;

                // Add to trail
                _context.ApprovalTrailEntries.Add(new ApprovalTrailEntry
                {
                    SiteVisitReportId = report.Id,
                    UserId = currentUserId,
                    UserRole = currentUserRole,
                    Action = "Locked",
                    Comments = $"Report locked for review",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Report locked successfully", 
                    lockedUntil = report.LockedUntil 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking report {ReportId}", id);
                return StatusCode(500, "An error occurred while locking the report");
            }
        }

        // QS: Add comment
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] ReportCommentDto dto)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                var report = await _context.SiteVisitReports.FindAsync(id);
                if (report == null)
                    return NotFound("Report not found");

                var comment = new ReportComment
                {
                    SiteVisitReportId = id,
                    ParentCommentId = dto.ParentCommentId,
                    UserId = currentUserId,
                    UserRole = currentUserRole,
                    CommentText = dto.CommentText
                };

                _context.ReportComments.Add(comment);
                await _context.SaveChangesAsync();

                return Ok(new { id = comment.Id, message = "Comment added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to report {ReportId}", id);
                return StatusCode(500, "An error occurred while adding comment");
            }
        }

        // QS: Return report to RM
        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnToRM(int id, [FromBody] QSActionDto dto)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var currentUserRole = "QS";

                var report = await _context.SiteVisitReports
                    .Include(r => r.Facility)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (report == null)
                    return NotFound("Report not found");

                // Verify lock
                if (report.CurrentQSLockedBy != currentUserId)
                {
                    return BadRequest("You must lock this report before returning it");
                }

                // Update report
                report.Status = "ReturnedToRM";
                report.CurrentQSLockedBy = null;
                report.LockedUntil = null;
                report.UpdatedBy = currentUserId;
                report.UpdatedAt = DateTime.UtcNow;

                // Add to trail
                _context.ApprovalTrailEntries.Add(new ApprovalTrailEntry
                {
                    SiteVisitReportId = report.Id,
                    UserId = currentUserId,
                    UserRole = currentUserRole,
                    Action = "Returned",
                    Comments = dto.Comments,
                    PreviousStatus = "UnderReview",
                    NewStatus = "ReturnedToRM",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Notify RM
                await _notificationService.NotifyRMReportStatusChangedAsync(
                    report.Id, 
                    report.RMUserId, 
                    "ReturnedToRM", 
                    dto.Comments);

                return Ok(new { message = "Report returned to RM successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning report {ReportId}", id);
                return StatusCode(500, "An error occurred while returning the report");
            }
        }

        // QS: Approve report
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveReport(int id, [FromBody] QSActionDto dto)
        {
            try
            {
                var currentUserId = User.Identity.Name;
                var currentUserRole = "QS";

                var report = await _context.SiteVisitReports
                    .Include(r => r.Facility)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (report == null)
                    return NotFound("Report not found");

                // Verify lock
                if (report.CurrentQSLockedBy != currentUserId)
                {
                    return BadRequest("You must lock this report before approving it");
                }

                // Update report
                report.Status = "Approved";
                report.ApprovedAt = DateTime.UtcNow;
                report.ApprovedBy = currentUserId;
                report.ApprovedAmount = dto.ApprovedAmount ?? report.RequestedAmount;
                report.CurrentQSLockedBy = null;
                report.LockedUntil = null;
                report.UpdatedBy = currentUserId;
                report.UpdatedAt = DateTime.UtcNow;

                // Add to trail
                _context.ApprovalTrailEntries.Add(new ApprovalTrailEntry
                {
                    SiteVisitReportId = report.Id,
                    UserId = currentUserId,
                    UserRole = currentUserRole,
                    Action = "Approved",
                    Comments = dto.Comments,
                    PreviousStatus = "UnderReview",
                    NewStatus = "Approved",
                    Timestamp = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Notify RM
                await _notificationService.NotifyRMReportStatusChangedAsync(
                    report.Id, 
                    report.RMUserId, 
                    "Approved", 
                    $"Approved amount: {dto.ApprovedAmount:C}. {dto.Comments}");

                return Ok(new { message = "Report approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving report {ReportId}", id);
                return StatusCode(500, "An error occurred while approving the report");
            }
        }

        // Helper methods
        private string DetermineAttachmentType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();
            
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".heic")
                return "Photo";
            
            if (fileName.Contains("valuation", StringComparison.OrdinalIgnoreCase))
                return "ValuationReport";
            
            if (fileName.Contains("interim", StringComparison.OrdinalIgnoreCase))
                return "InterimCertificate";
            
            if (fileName.Contains("drawdown", StringComparison.OrdinalIgnoreCase))
                return "DrawdownInstruction";
            
            return "Other";
        }

        private SiteVisitReportResponseDto MapToResponseDto(SiteVisitReport report)
        {
            return new SiteVisitReportResponseDto
            {
                Id = report.Id,
                IBPSNumber = report.Facility?.IBPSNumber,
                CustomerName = report.Facility?.CustomerName,
                VisitDate = report.VisitDate,
                RMUserId = report.RMUserId,
                Status = report.Status,
                RequestedAmount = report.RequestedAmount,
                ApprovedAmount = report.ApprovedAmount,
                CurrentQSLockedBy = report.CurrentQSLockedBy,
                LockedUntil = report.LockedUntil,
                CreatedAt = report.CreatedAt,
                SubmittedAt = report.SubmittedAt,
                ApprovedAt = report.ApprovedAt,
                Attachments = report.Attachments?.Select(a => new AttachmentInfoDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    FileSizeInBytes = a.FileSizeInBytes,
                    AttachmentType = a.AttachmentType,
                    IsGeoTagged = a.IsGeoTagged
                }).ToList()
            };
        }
    }
}