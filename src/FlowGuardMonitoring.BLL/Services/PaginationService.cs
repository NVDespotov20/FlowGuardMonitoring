using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.DAL.Repositories;

namespace FlowGuardMonitoring.BLL.Services;

public class PaginationService<T> : IPaginationService<T>
    where T : class
{
    private readonly IRepository<T> repository;
    public PaginationService(IRepository<T> repository)
    {
        this.repository = repository;
    }

    public async Task<PaginatedResult<T>> GetPaginatedRecords(
        int pageNumber, int pageSize, string sortColumn, string sortDirection, string searchValue)
    {
        // Get paginated records with search and sort options
        List<T> records = await this.repository.GetPagedAsync(pageNumber, pageSize, sortColumn, sortDirection, searchValue);

        // Get total record count before applying pagination
        var totalRecords = this.repository.GetCount(searchValue);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PaginatedResult<T>
        {
            Records = records,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
        };
    }
}
