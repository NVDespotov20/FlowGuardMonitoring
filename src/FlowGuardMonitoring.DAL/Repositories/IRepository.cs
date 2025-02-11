using System.Collections.Generic;
using System.Threading.Tasks;
using FlowGuardMonitoring.DAL.Models;

namespace FlowGuardMonitoring.DAL.Repositories;
public interface IRepository<T>
    where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    int GetCount();
}