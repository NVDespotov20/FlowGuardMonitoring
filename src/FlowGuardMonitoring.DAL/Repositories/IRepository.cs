using System.Collections.Generic;
using System.Threading.Tasks;
using FlowGuardMonitoring.DAL.Models;

namespace FlowGuardMonitoring.DAL.Repositories;
public interface IRepository<T>
    where T : class
{
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string sortColumn,
        string sortDirection,
        string searchValue,
        string userId,
        int? id = null);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    public int GetCount(string userId, string searchValue = "");
}