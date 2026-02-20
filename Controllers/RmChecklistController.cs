// Controllers/RmChecklistController.cs
using System.Text.Json;
using geoback.Data;
using geoback.DTOs;
using geoback.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;

namespace geoback.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RmChecklistController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    public class UploadChecklistPhotoRequest
    {
        public IFormFile? File { get; set; }
        public string? Section { get; set; }
        public int? Slot { get; set; }
    }

    public RmChecklistController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    [HttpPost("photos")]
    [RequestSizeLimit(10_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UploadChecklistPhoto([FromForm] UploadChecklistPhotoRequest request)
    {
        var file = request.File;
        var section = request.Section;
        var slot = request.Slot;

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Photo file is required." });
        }

        if (file.Length > 10_000_000)
        {
            return BadRequest(new { message = "Maximum allowed photo size is 10MB." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only JPG, PNG and WEBP files are allowed." });
        }

        var uploadsRoot = Path.Combine(_environment.ContentRootPath, "uploads", "rm-checklist-photos");
        Directory.CreateDirectory(uploadsRoot);

        var safeSection = string.IsNullOrWhiteSpace(section)
            ? "general"
            : new string(section.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray()).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeSection))
        {
            safeSection = "general";
        }

        var generatedFileName = $"{safeSection}-slot{slot ?? 0}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, generatedFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var publicUrl = $"/api/rmChecklist/photos/{generatedFileName}";

        return Ok(new
        {
            message = "Photo uploaded successfully",
            url = publicUrl,
            fileName = generatedFileName,
            section = safeSection,
            slot,
        });
    }

    [HttpGet("photos/{fileName}")]
    public IActionResult GetChecklistPhoto(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            return BadRequest(new { message = "Invalid file name." });
        }

        var fullPath = Path.Combine(_environment.ContentRootPath, "uploads", "rm-checklist-photos", safeFileName);
        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound(new { message = "Photo not found." });
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(safeFileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(fullPath, contentType);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChecklistResponseDto>>> GetAllRmChecklists()
    {
        var checklists = await _context.Checklists
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var rmIds = checklists
            .Where(c => c.AssignedToRM.HasValue)
            .Select(c => c.AssignedToRM!.Value)
            .Distinct()
            .ToList();

        var rmMap = await _context.Users
            .Where(u => rmIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new ChecklistUserRefDto
            {
                Id = u.Id,
                Name = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email,
            });

        var result = checklists.Select(c => new ChecklistResponseDto
        {
            Id = c.Id,
            DclNo = c.DclNo,
            CustomerId = c.CustomerId,
            CustomerNumber = c.CustomerNumber,
            CustomerName = c.CustomerName,
            CustomerEmail = c.CustomerEmail,
            ProjectName = c.ProjectName,
            IbpsNo = c.IbpsNo,
            Status = c.Status,
            AssignedToRM = c.AssignedToRM.HasValue && rmMap.ContainsKey(c.AssignedToRM.Value)
                ? rmMap[c.AssignedToRM.Value]
                : null,
            Documents = DeserializeDocuments(c.DocumentsJson),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            SiteVisitForm = DeserializeSiteVisitForm(c.SiteVisitFormJson)
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChecklistResponseDto>> GetChecklistById(Guid id)
    {
        var checklist = await _context.Checklists
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist == null)
        {
            return NotFound(new { message = "Checklist not found." });
        }

        // Get RM info if assigned
        ChecklistUserRefDto? assignedRM = null;
        if (checklist.AssignedToRM.HasValue)
        {
            var rm = await _context.Users
                .Where(u => u.Id == checklist.AssignedToRM.Value)
                .Select(u => new ChecklistUserRefDto
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email,
                })
                .FirstOrDefaultAsync();
            assignedRM = rm;
        }

        var result = new ChecklistResponseDto
        {
            Id = checklist.Id,
            DclNo = checklist.DclNo,
            CustomerId = checklist.CustomerId,
            CustomerNumber = checklist.CustomerNumber,
            CustomerName = checklist.CustomerName,
            CustomerEmail = checklist.CustomerEmail,
            ProjectName = checklist.ProjectName,
            IbpsNo = checklist.IbpsNo,
            Status = checklist.Status,
            AssignedToRM = assignedRM,
            Documents = DeserializeDocuments(checklist.DocumentsJson),
            CreatedAt = checklist.CreatedAt,
            UpdatedAt = checklist.UpdatedAt,
            SiteVisitForm = DeserializeSiteVisitForm(checklist.SiteVisitFormJson)
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateChecklist([FromBody] CreateChecklistDto payload)
    {
        var resolvedProjectName = !string.IsNullOrWhiteSpace(payload.ProjectName)
            ? payload.ProjectName
            : payload.LoanType ?? string.Empty;

        if (string.IsNullOrWhiteSpace(payload.CustomerNumber) ||
            string.IsNullOrWhiteSpace(payload.CustomerName) ||
            string.IsNullOrWhiteSpace(resolvedProjectName) ||
            payload.AssignedToRM == null ||
            string.IsNullOrWhiteSpace(payload.IbpsNo))
        {
            return BadRequest(new { message = "Please fill all required fields." });
        }

        var existingCrnNumbers = await _context.Checklists
            .AsNoTracking()
            .Where(c => c.DclNo.StartsWith("CRN-"))
            .Select(c => c.DclNo)
            .ToListAsync();

        var nextNumber = existingCrnNumbers
            .Select(ExtractCrnSequence)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var dclNo = $"CRN-{nextNumber:000}";

        var checklist = new Checklist
        {
            DclNo = dclNo,
            CustomerId = payload.CustomerId,
            CustomerNumber = payload.CustomerNumber,
            CustomerName = payload.CustomerName,
            CustomerEmail = payload.CustomerEmail,
            ProjectName = resolvedProjectName,
            IbpsNo = payload.IbpsNo,
            AssignedToRM = payload.AssignedToRM,
            Status = "pending",
            DocumentsJson = JsonSerializer.Serialize(payload.Documents, _jsonOptions),
            SiteVisitFormJson = payload.SiteVisitForm != null 
                ? JsonSerializer.Serialize(payload.SiteVisitForm, _jsonOptions)
                : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.Checklists.Add(checklist);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChecklistById), new { id = checklist.Id }, new
        {
            message = "Checklist created successfully",
            checklist = new
            {
                id = checklist.Id,
                _id = checklist.Id,
                checklist.DclNo,
                checklist.CustomerName,
                checklist.CustomerNumber,
                checklist.CustomerEmail,
                checklist.ProjectName,
                loanType = checklist.ProjectName,
                checklist.IbpsNo,
                checklist.Status,
                documents = payload.Documents,
                checklist.CreatedAt,
                checklist.UpdatedAt,
            },
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateChecklist(Guid id, [FromBody] UpdateChecklistDto payload)
    {
        var checklist = await _context.Checklists.FirstOrDefaultAsync(c => c.Id == id);

        if (checklist == null)
        {
            return NotFound(new { message = "Checklist not found." });
        }

        var resolvedProjectName = !string.IsNullOrWhiteSpace(payload.ProjectName)
            ? payload.ProjectName
            : payload.LoanType ?? string.Empty;

        if (string.IsNullOrWhiteSpace(payload.CustomerNumber) ||
            string.IsNullOrWhiteSpace(payload.CustomerName) ||
            string.IsNullOrWhiteSpace(resolvedProjectName) ||
            payload.AssignedToRM == null ||
            string.IsNullOrWhiteSpace(payload.IbpsNo))
        {
            return BadRequest(new { message = "Please fill all required fields." });
        }

        var normalizedStatus = NormalizeWorkflowStatus(payload.Status, checklist.Status);

        checklist.CustomerId = payload.CustomerId;
        checklist.CustomerNumber = payload.CustomerNumber.Trim();
        checklist.CustomerName = payload.CustomerName.Trim();
        checklist.CustomerEmail = payload.CustomerEmail;
        checklist.ProjectName = resolvedProjectName.Trim();
        checklist.IbpsNo = payload.IbpsNo.Trim();
        checklist.AssignedToRM = payload.AssignedToRM;
        checklist.Status = normalizedStatus;
        checklist.DocumentsJson = JsonSerializer.Serialize(payload.Documents, _jsonOptions);
        checklist.SiteVisitFormJson = payload.SiteVisitForm != null 
            ? JsonSerializer.Serialize(payload.SiteVisitForm, _jsonOptions)
            : checklist.SiteVisitFormJson;
        checklist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Checklist updated successfully",
            checklist = new
            {
                id = checklist.Id,
                _id = checklist.Id,
                checklist.DclNo,
                checklist.CustomerName,
                checklist.CustomerNumber,
                checklist.CustomerEmail,
                checklist.ProjectName,
                loanType = checklist.ProjectName,
                checklist.IbpsNo,
                checklist.Status,
                documents = payload.Documents,
                checklist.CreatedAt,
                checklist.UpdatedAt,
            },
        });
    }

    private static List<ChecklistDocumentCategoryDto> DeserializeDocuments(string documentsJson)
    {
        if (string.IsNullOrWhiteSpace(documentsJson))
        {
            return new List<ChecklistDocumentCategoryDto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ChecklistDocumentCategoryDto>>(documentsJson) ?? new List<ChecklistDocumentCategoryDto>();
        }
        catch
        {
            return new List<ChecklistDocumentCategoryDto>();
        }
    }

    private static object? DeserializeSiteVisitForm(string? siteVisitFormJson)
    {
        if (string.IsNullOrWhiteSpace(siteVisitFormJson) || siteVisitFormJson == "null")
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<object>(siteVisitFormJson);
        }
        catch
        {
            return null;
        }
    }

    private static int ExtractCrnSequence(string? callReportNumber)
    {
        if (string.IsNullOrWhiteSpace(callReportNumber))
        {
            return 0;
        }

        if (!callReportNumber.StartsWith("CRN-", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var numberPart = callReportNumber[4..];
        return int.TryParse(numberPart, out var parsedNumber) && parsedNumber > 0
            ? parsedNumber
            : 0;
    }

    private static string NormalizeWorkflowStatus(string? requestedStatus, string currentStatus)
    {
        var status = (requestedStatus ?? string.Empty).Trim().ToLowerInvariant();

        return status switch
        {
            "draft" => "draft",
            "pending_qs_review" => "pending_qs_review",
            "pendingqsreview" => "pending_qs_review",
            _ => string.IsNullOrWhiteSpace(currentStatus) ? "pending" : currentStatus,
        };
    }
}