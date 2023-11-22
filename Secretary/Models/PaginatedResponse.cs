namespace Secretary.Models;

/// <summary>
/// Paginated response
/// </summary>
/// <typeparam name="T"><see cref="T"/></typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Current page
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size. Default: 10
    /// </summary>
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Total items
    /// </summary>
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    
    /// <summary>
    /// Array of data <see cref="T"/>
    /// </summary>
    public IEnumerable<T> Data { get; set; }
}