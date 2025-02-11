using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Repositories;

public class SensorRepository(FlowGuardMonitoringContext context) : IRepository<Sensor>
{
    public async Task<IEnumerable<Sensor>> GetAllAsync()
    {
        return await context.Sensors.ToListAsync();
    }

    public async Task<List<Sensor>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await context.Sensors
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Site)
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

    public int GetCount()
    {
        return context.Sensors.Count();
    }
}