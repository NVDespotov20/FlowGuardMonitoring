using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Repositories;

public class SensorRepository(FlowGuardMonitoringContext context) : IRepository<Sensor>
{
    public async Task<List<Sensor>> GetAllAsync()
    {
        return await context.Sensors.ToListAsync();
    }

    public async Task<List<Sensor>> GetPagedAsync(int pageNumber, int pageSize, string sortColumn, string sortDirection, string searchValue)
    {
        // Start with the base query
        var query = context.Sensors
            .Include(s => s.Site) // Assuming a Sensor has a relationship with Site
            .AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(searchValue))
        {
            query = query.Where(s =>
                s.Name.Contains(searchValue) ||
                s.Type.ToString().Contains(searchValue) ||
                s.Site.Name.Contains(searchValue) ||
                s.InstallationDate.ToString().Contains(searchValue) ||
                s.IsActive.ToString().Contains(searchValue));
        }

        switch (sortColumn.ToLower())
        {
            case "name":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name);
                break;
            case "type":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Type) : query.OrderByDescending(s => s.Type);
                break;
            case "installationdate":
                query = sortDirection == "asc" ? query.OrderBy(s => s.InstallationDate) : query.OrderByDescending(s => s.InstallationDate);
                break;
            case "isactive":
                query = sortDirection == "asc" ? query.OrderBy(s => s.IsActive) : query.OrderByDescending(s => s.IsActive);
                break;
            case "sitename":
                query = sortDirection == "asc" ? query.OrderBy(s => s.Site.Name) : query.OrderByDescending(s => s.Site.Name);
                break;
            default:
                query = sortDirection == "asc" ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name);
                break;
        }

        // Apply pagination
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Sensor?> GetByIdAsync(int id)
    {
        return await context.Sensors.FindAsync(id);
    }

    public async Task AddAsync(Sensor sensor)
    {
        await context.Sensors.AddAsync(sensor);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sensor sensor)
    {
        context.Sensors.Update(sensor);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var sensor = await context.Sensors.FindAsync(id);
        if (sensor != null)
        {
            context.Sensors.Remove(sensor);
            await context.SaveChangesAsync();
        }
    }

    public int GetCount(string searchValue)
    {
        IQueryable<Sensor>? query = context.Sensors.AsQueryable();

        if (!string.IsNullOrEmpty(searchValue))
        {
            query = query.Where(s =>
                s.Name.Contains(searchValue) ||
                s.Type.ToString().Contains(searchValue) ||
                s.Site.Name.Contains(searchValue) ||
                s.InstallationDate.ToString().Contains(searchValue) ||
                s.IsActive.ToString().Contains(searchValue));
        }

        return query.Count();
    }
}