namespace geoback.Models;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
