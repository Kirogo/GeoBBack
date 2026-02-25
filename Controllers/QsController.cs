// Controllers/QsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using geoback.Data;
using geoback.Models;
using System.Security.Claims;

namespace geoback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QsController> _logger;

        public QsController(ApplicationDbContext context, ILogger<QsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/qs/dashboard/stats
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var stats = new
                {
                    PendingReviews = await _context.Checklists
                        .CountAsync(c => c.Status == "Submitted" || c.Status == "PendingQSReview"),
                    InProgress = await _context.Checklists
                        .CountAsync(c => c.Status == "UnderReview" || c.Status == "InReview"),
                    CompletedToday = await _context.Checklists
                        .CountAsync(c => c.Status == "Approved" && 
                            c.UpdatedAt.Date == DateTime.UtcNow.Date),
                    ScheduledVisits = 0,
                    AverageResponseTime = await CalculateAverageResponseTime(),
                    CriticalIssues = await _context.Checklists
                        .CountAsync(c => c.Priority == "High" || c.Priority == "Critical"),
                    MyActiveReviews = await _context.Checklists
                        .CountAsync(c => c.AssignedToQS == userId && 
                            (c.Status == "UnderReview" || c.Status == "InReview")),
                    OverdueReviews = await _context.Checklists
                        .CountAsync(c => c.Status == "UnderReview" && 
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
                .Where(c => c.Status == "Approved" && c.SubmittedAt != null && c.ReviewedAt != null)
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
        public async Task<ActionResult<PaginatedResponse<Checklist>>> GetPendingReviews(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Checklists
                    .Where(c => c.Status == "Submitted" || c.Status == "PendingQSReview")
                    .OrderByDescending(c => c.SubmittedAt ?? c.CreatedAt);

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new PaginatedResponse<Checklist>
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
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
        public async Task<ActionResult<PaginatedResponse<Checklist>>> GetInProgressReviews(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Checklists
                    .Where(c => c.Status == "UnderReview" || c.Status == "InReview")
                    .OrderByDescending(c => c.UpdatedAt);

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new PaginatedResponse<Checklist>
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
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
        public async Task<ActionResult<PaginatedResponse<Checklist>>> GetCompletedReviews(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Checklists
                    .Where(c => c.Status == "Approved" || c.Status == "Completed")
                    .OrderByDescending(c => c.UpdatedAt);

                var total = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new PaginatedResponse<Checklist>
                {
                    Items = items,
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize)
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
        public async Task<ActionResult<List<Checklist>>> GetMyActiveReviews()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var reviews = await _context.Checklists
                    .Where(c => c.AssignedToQS == userId && 
                        (c.Status == "UnderReview" || c.Status == "InReview"))
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my active reviews");
                return StatusCode(500, new { message = "Error fetching your active reviews" });
            }
        }

        // GET: api/qs/reviews/{id}
        [HttpGet("reviews/{id}")]
        public async Task<ActionResult<Checklist>> GetReportDetails(Guid id)
        {
            try
            {
                var report = await _context.Checklists
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (report == null)
                    return NotFound(new { message = $"Report with ID {id} not found" });

                return Ok(report);
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                    return Unauthorized(new { message = "User not authenticated" });

                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    ReportId = id,
                    UserId = Guid.Parse(userId),
                    UserName = userName,
                    UserRole = userRole ?? "QS",
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                    return Unauthorized(new { message = "User not authenticated" });

                var report = await _context.Checklists.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = $"Report with ID {id} not found" });

                report.AssignedToQS = userId;
                report.AssignedToQSName = userName;
                report.Status = "UnderReview";
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                    return Unauthorized(new { message = "User not authenticated" });

                var report = await _context.Checklists.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = $"Report with ID {id} not found" });

                report.Status = "RevisionRequested";
                report.UpdatedAt = DateTime.UtcNow;

                // Add revision comment
                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    ReportId = id,
                    UserId = Guid.Parse(userId),
                    UserName = userName,
                    UserRole = "QS",
                    Text = $"Revision requested: {dto.Notes}\nRequired changes: {string.Join(", ", dto.RequiredChanges)}",
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                    return Unauthorized(new { message = "User not authenticated" });

                var report = await _context.Checklists.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = $"Report with ID {id} not found" });

                report.Status = "Approved";
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
                        UserName = userName,
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                    return Unauthorized(new { message = "User not authenticated" });

                var report = await _context.Checklists.FindAsync(id);
                if (report == null)
                    return NotFound(new { message = $"Report with ID {id} not found" });

                report.Status = "Rejected";
                report.ReviewedAt = DateTime.UtcNow;
                report.ReviewedBy = userId;
                report.UpdatedAt = DateTime.UtcNow;

                // Add rejection comment
                var comment = new Comment
                {
                    Id = Guid.NewGuid(),
                    ReportId = id,
                    UserId = Guid.Parse(userId),
                    UserName = userName,
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
    }

    // DTO Classes
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

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
}