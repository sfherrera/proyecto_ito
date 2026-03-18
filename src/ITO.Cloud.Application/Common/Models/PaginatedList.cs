namespace ITO.Cloud.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PaginatedList<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize) =>
        new() { Items = [..items], TotalCount = totalCount, Page = page, PageSize = pageSize };
}

public record PaginationParams(int Page = 1, int PageSize = 20, string? Search = null, string? SortBy = null, bool SortDesc = false)
{
    public int Skip => (Page - 1) * PageSize;
}
