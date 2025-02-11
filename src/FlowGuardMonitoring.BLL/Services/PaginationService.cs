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
        int pageNumber, int pageSize)
    {
        List<T> records = await this.repository.GetPagedAsync(pageNumber, pageSize);

        var totalRecords = this.repository.GetCount();
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
