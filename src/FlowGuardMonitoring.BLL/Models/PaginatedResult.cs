using System.Collections.Generic;

namespace FlowGuardMonitoring.BLL.Models;

public class PaginatedResult<T>
{
    public List<T> Records { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
