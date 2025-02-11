using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using FlowGuardMonitoring.DAL;
using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Repositories;

    public class MeasurementRepository(FlowGuardMonitoringContext context) : IRepository<Measurement>
    {
        public async Task<IEnumerable<Measurement>> GetAllAsync()
        {
            return await context.Measurements.ToListAsync();
        }

        public async Task<List<Measurement>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await context.Measurements
                .OrderBy(m => m.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Measurement?> GetByIdAsync(int id)
        {
            return await context.Measurements.FirstOrDefaultAsync(m => m.MeasurementId == id);
        }

        public async Task AddAsync(Measurement entity)
        {
            await context.Measurements.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Measurement entity)
        {
            context.Measurements.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await this.GetByIdAsync(id);
            if (entity != null)
            {
                context.Measurements.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task GetPage()
        {
            var page = await context.Measurements.ToListAsync();
        }

        public int GetCount()
        {
            return context.Measurements.Count();
        }
    }
