namespace OrderFlow.Application._Shared;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items, 
    int Page,
    int PageSize, 
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
