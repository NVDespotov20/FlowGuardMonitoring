using System.Threading.Tasks;
using FlowGuardMonitoring.BLL.Models;

namespace FlowGuardMonitoring.BLL.Contracts;

public interface IPaginationService<T>
{
    Task<PaginatedResult<T>> GetPaginatedRecords(
        int pageNumber,
        int pageSize,
        string sortColumn,
        string sortDirection,
        string searchValue,
        string userId,
        int? entityId = null);
}
