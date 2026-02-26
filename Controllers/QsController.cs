// Controllers/QsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using geoback.Data;
using geoback.Models;
using geoback.DTOs;
using System.Text.Json;
using System.Security.Claims;

namespace geoback.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QsController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public QsController(ApplicationDbContext context, ILogger<QsController> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    // Helper method to get current user ID from token
    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? 
               $"{User.FindFirst("firstName")?.Value} {User.FindFirst("lastName")?.Value}".Trim();
    }

    // GET: api/qs/dashboard/stats
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<object>> GetDashboardStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var stats = new
            {
                PendingReviews = await _context.Checklists
                    .CountAsync(c => c.Status == "submitted" || c.Status == "pending_qs_review"),
                InProgress = await _context.Checklists
                    .CountAsync(c => c.Status == "under_review" || c.Status == "underreview"),
                CompletedToday = await _context.Checklists
                    .CountAsync(c => c.Status == "approved" && 
                        c.UpdatedAt.Date == DateTime.UtcNow.Date),
                ScheduledVisits = 0, // To be implemented
                AverageResponseTime = await CalculateAverageResponseTime(),
                CriticalIssues = await _context.Checklists
                    .CountAsync(c => c.Priority == "High" || c.Priority == "Critical"),
                MyActiveReviews = await _context.Checklists
                    .CountAsync(c => c.AssignedToQS == userId && 
                        (c.Status == "under_review" || c.Status == "underreview")),
                OverdueReviews = await _context.Checklists
                    .CountAsync(c => c.Status == "under_review" && 
                        c.UpdatedAt < DateTime.UtcNow.AddDays(-2))
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return StatusCode(500, new { message = "Error fetching dashboard statistics" });
        }
    }

    private async Task<string> CalculateAverageResponseTime()
    {
        var approvedReports = await _context.Checklists
            .Where(c => c.Status == "approved" && c.SubmittedAt != null && c.ReviewedAt != null)
            .ToListAsync();

        if (!approvedReports.Any())
            return "0h";

        var totalHours = approvedReports
            .Select(c => (c.ReviewedAt!.Value - c.SubmittedAt!.Value).TotalHours)
            .Average();

        return $"{Math.Round(totalHours)}h";
    }

    // GET: api/qs/reviews/pending
    [HttpGet("reviews/pending")]
    public async Task<ActionResult<object>> GetPendingReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Checklists
                .Where(c => c.Status == "submitted" || c.Status == "pending_qs_review")
                .OrderByDescending(c => c.SubmittedAt ?? c.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDtos = items.Select(c => MapToReportDto(c)).ToList();

            return Ok(new
            {
                items = reportDtos,
                total = total,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending reviews");
            return StatusCode(500, new { message = "Error fetching pending reviews" });
        }
    }

    // GET: api/qs/reviews/in-progress
    [HttpGet("reviews/in-progress")]
    public async Task<ActionResult<object>> GetInProgressReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Checklists
                .Where(c => c.Status == "under_review" || c.Status == "underreview")
                .OrderByDescending(c => c.UpdatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDtos = items.Select(c => MapToReportDto(c)).ToList();

            return Ok(new
            {
                items = reportDtos,
                total = total,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting in-progress reviews");
            return StatusCode(500, new { message = "Error fetching in-progress reviews" });
        }
    }

    // GET: api/qs/reviews/completed
    [HttpGet("reviews/completed")]
    public async Task<ActionResult<object>> GetCompletedReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Checklists
                .Where(c => c.Status == "approved" || c.Status == "completed")
                .OrderByDescending(c => c.UpdatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDtos = items.Select(c => MapToReportDto(c)).ToList();

            return Ok(new
            {
                items = reportDtos,
                total = total,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed reviews");
            return StatusCode(500, new { message = "Error fetching completed reviews" });
        }
    }

    // GET: api/qs/reviews/my-active
    [HttpGet("reviews/my-active")]
    public async Task<ActionResult<List<object>>> GetMyActiveReviews()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var reviews = await _context.Checklists
                .Where(c => c.AssignedToQS == userId && 
                    (c.Status == "under_review" || c.Status == "underreview"))
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            return Ok(reviews.Select(c => MapToReportDto(c)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my active reviews");
            return StatusCode(500, new { message = "Error fetching your active reviews" });
        }
    }

    // GET: api/qs/reviews/{id}
    [HttpGet("reviews/{id}")]
    public async Task<ActionResult<object>> GetReportDetails(Guid id)
    {
        try
        {
            var report = await _context.Checklists
                .FirstOrDefaultAsync(c => c.Id == id);

            if (report == null)
                return NotFound(new { message = $"Report with ID {id} not found" });

            return Ok(MapToReportDto(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report details: {ReportId}", id);
            return StatusCode(500, new { message = "Error fetching report details" });
        }
    }

    // GET: api/qs/reviews/{id}/comments
    [HttpGet("reviews/{id}/comments")]
    public async Task<ActionResult<List<Comment>>> GetReportComments(Guid id)
    {
        try
        {
            var comments = await _context.Comments
                .Where(c => c.ReportId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for report: {ReportId}", id);
            return StatusCode(500, new { message = "Error fetching comments" });
        }
    }

    // POST: api/qs/reviews/{id}/comments
    [HttpPost("reviews/{id}/comments")]
    public async Task<ActionResult<Comment>> AddComment(Guid id, [FromBody] AddCommentDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "QS";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ReportId = id,
                UserId = Guid.Parse(userId),
                UserName = userName ?? "QS User",
                UserRole = userRole,
                Text = dto.Comment,
                IsInternal = dto.IsInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to report: {ReportId}", id);
            return StatusCode(500, new { message = "Error adding comment" });
        }
    }

    // POST: api/qs/reviews/{id}/assign
    [HttpPost("reviews/{id}/assign")]
    public async Task<IActionResult> AssignToMe(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var report = await _context.Checklists.FindAsync(id);
            if (report == null)
                return NotFound(new { message = $"Report with ID {id} not found" });

            report.AssignedToQS = userId;
            report.AssignedToQSName = userName;
            report.Status = "under_review";
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Report assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning report: {ReportId}", id);
            return StatusCode(500, new { message = "Error assigning report" });
        }
    }

    // POST: api/qs/reviews/{id}/revision
    [HttpPost("reviews/{id}/revision")]
    public async Task<IActionResult> RequestRevision(Guid id, [FromBody] RevisionRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var report = await _context.Checklists.FindAsync(id);
            if (report == null)
                return NotFound(new { message = $"Report with ID {id} not found" });

            // Update report status to revision_requested
            report.Status = "revision_requested";
            report.UpdatedAt = DateTime.UtcNow;

            // Add revision comment with the notes
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ReportId = id,
                UserId = Guid.Parse(userId),
                UserName = userName ?? "QS User",
                UserRole = "QS",
                Text = dto.Notes, // Just store the notes as the comment text
                IsInternal = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Revision requested successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting revision for report: {ReportId}", id);
            return StatusCode(500, new { message = "Error requesting revision" });
        }
    }

    // POST: api/qs/reviews/{id}/approve
    [HttpPost("reviews/{id}/approve")]
    public async Task<IActionResult> ApproveReport(Guid id, [FromBody] ApproveReportDto? dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var report = await _context.Checklists.FindAsync(id);
            if (report == null)
                return NotFound(new { message = $"Report with ID {id} not found" });

            report.Status = "approved";
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewedBy = userId;
            report.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto?.Notes))
            {
                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    ReportId = id,
                    UserId = Guid.Parse(userId),
                    UserName = userName ?? "QS User",
                    UserRole = "QS",
                    Text = $"Approved: {dto.Notes}",
                    IsInternal = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Comments.Add(comment);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Report approved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving report: {ReportId}", id);
            return StatusCode(500, new { message = "Error approving report" });
        }
    }

    // POST: api/qs/reviews/{id}/reject
    [HttpPost("reviews/{id}/reject")]
    public async Task<IActionResult> RejectReport(Guid id, [FromBody] RejectReportDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userName = GetCurrentUserName();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var report = await _context.Checklists.FindAsync(id);
            if (report == null)
                return NotFound(new { message = $"Report with ID {id} not found" });

            report.Status = "rejected";
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewedBy = userId;
            report.UpdatedAt = DateTime.UtcNow;

            // Add rejection comment
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ReportId = id,
                UserId = Guid.Parse(userId),
                UserName = userName ?? "QS User",
                UserRole = "QS",
                Text = $"Rejected: {dto.Reason}",
                IsInternal = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Report rejected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting report: {ReportId}", id);
            return StatusCode(500, new { message = "Error rejecting report" });
        }
    }

    // GET: api/qs/site-visits/upcoming
    [HttpGet("site-visits/upcoming")]
    public async Task<ActionResult<List<object>>> GetUpcomingSiteVisits()
    {
        try
        {
            // Return empty list for now - implement when site visits are added
            return Ok(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming site visits");
            return StatusCode(500, new { message = "Error fetching site visits" });
        }
    }

    // Helper method to map Checklist to a DTO that matches what the frontend expects
    private object MapToReportDto(Checklist checklist)
    {
        // Parse the SiteVisitFormJson if it exists
        object? siteVisitForm = null;
        if (!string.IsNullOrWhiteSpace(checklist.SiteVisitFormJson) && checklist.SiteVisitFormJson != "null")
        {
            try
            {
                siteVisitForm = JsonSerializer.Deserialize<object>(checklist.SiteVisitFormJson);
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        // Parse DocumentsJson
        var documents = new List<object>();
        if (!string.IsNullOrWhiteSpace(checklist.DocumentsJson) && checklist.DocumentsJson != "[]")
        {
            try
            {
                documents = JsonSerializer.Deserialize<List<object>>(checklist.DocumentsJson) ?? new List<object>();
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        return new
        {
            id = checklist.Id,
            reportNo = checklist.DclNo,
            customerId = checklist.CustomerId,
            customerNumber = checklist.CustomerNumber,
            customerName = checklist.CustomerName,
            customerEmail = checklist.CustomerEmail,
            projectName = checklist.ProjectName,
            ibpsNo = checklist.IbpsNo,
            status = checklist.Status,
            rmId = checklist.AssignedToRM,
            rmName = GetRmName(checklist.AssignedToRM).Result,
            documents = documents,
            siteVisitForm = siteVisitForm,
            isLocked = checklist.IsLocked,
            lockedBy = checklist.LockedByUserId.HasValue ? new
            {
                id = checklist.LockedByUserId,
                name = checklist.LockedByUserName
            } : null,
            lockedAt = checklist.LockedAt,
            assignedToQS = checklist.AssignedToQS,
            assignedToQSName = checklist.AssignedToQSName,
            submittedAt = checklist.SubmittedAt,
            priority = checklist.Priority,
            reviewedAt = checklist.ReviewedAt,
            reviewedBy = checklist.ReviewedBy,
            createdAt = checklist.CreatedAt,
            updatedAt = checklist.UpdatedAt
        };
    }

    private async Task<string?> GetRmName(Guid? rmId)
    {
        if (!rmId.HasValue) return null;
        
        var user = await _context.Users.FindAsync(rmId.Value);
        return user != null ? $"{user.FirstName} {user.LastName}".Trim() : null;
    }
}

// DTOs for QS endpoints
public class AddCommentDto
{
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public class RevisionRequestDto
{
    public string Notes { get; set; } = string.Empty;
    public string[] RequiredChanges { get; set; } = Array.Empty<string>();
}

public class ApproveReportDto
{
    public string? Notes { get; set; }
}

public class RejectReportDto
{
    public string Reason { get; set; } = string.Empty;
}