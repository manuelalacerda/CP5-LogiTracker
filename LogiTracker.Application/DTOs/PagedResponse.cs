namespace LogiTracker.Application.DTOs;

public record PagedResponse<T>(
    int Page, int PageSize, int TotalItems, int TotalPages,
    IReadOnlyList<T> Items, bool HasPrevious, bool HasNext)
{
    public static PagedResponse<T> Create(int page, int pageSize, int totalItems, IReadOnlyList<T> items)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new(page, pageSize, totalItems, totalPages, items, page > 1, page < totalPages);
    }
}
