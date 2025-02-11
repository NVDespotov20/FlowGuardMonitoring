using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Repositories;

public class SiteRepository(FlowGuardMonitoringContext context) : IRepository<Site>
{
    public async Task<IEnumerable<Site>> GetAllAsync()
    {
        return await context.Sites.ToListAsync();
    }

    public async Task<List<Site>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await context.Sites
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Site?> GetByIdAsync(int id)
    {
        return await context.Sites.FindAsync(id);
    }

    public async Task AddAsync(Site site)
    {
        await context.Sites.AddAsync(site);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Site site)
    {
        context.Sites.Update(site);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var site = await context.Sites.FindAsync(id);
        if (site != null)
        {
            context.Sites.Remove(site);
            await context.SaveChangesAsync();
        }
    }

    public int GetCount()
    {
        return context.Sites.Count();
    }
}