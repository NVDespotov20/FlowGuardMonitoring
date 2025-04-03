using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Repositories;

public class SiteRepository(FlowGuardMonitoringContext context) : IRepository<Site>
{
    public async Task<List<Site>> GetAllAsync()
    {
        return await context.Sites.ToListAsync();
    }

    public async Task<List<Site>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string sortColumn,
        string sortDirection,
        string searchValue,
        string userId,
        int? id = null)
    {
        var query = context.Sites
            .Where(s => s.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchValue))
        {
            query = query.Where(s =>
                s.Name.Contains(searchValue) ||
                s.Description.Contains(searchValue) ||
                s.Latitude.ToString().Contains(searchValue) ||
                s.Longitude.ToString().Contains(searchValue));
        }

        switch (sortColumn.ToLower())
        {
            case "name":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name);
                break;
            case "description":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Description) : query.OrderByDescending(s => s.Description);
                break;
            case "latitude":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Latitude) : query.OrderByDescending(s => s.Latitude);
                break;
            case "longitude":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Longitude) : query.OrderByDescending(s => s.Longitude);
                break;
            default:
                query = sortDirection == "asc" ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name);
                break;
        }

        return await query
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

    public int GetCount(string userId, string searchValue)
    {
        var query = context.Sites
            .Where(s => s.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchValue))
        {
            query = query.Where(s =>
                s.Name.Contains(searchValue) ||
                s.Description.Contains(searchValue) ||
                s.Latitude.ToString().Contains(searchValue) ||
                s.Longitude.ToString().Contains(searchValue));
        }

        return query.Count();
    }
}