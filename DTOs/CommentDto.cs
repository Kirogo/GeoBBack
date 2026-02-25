// DTOs/CommentDtos.cs
using System.Text.Json.Serialization;

namespace geoback.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentDto
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

    public class PaginatedResponseDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}