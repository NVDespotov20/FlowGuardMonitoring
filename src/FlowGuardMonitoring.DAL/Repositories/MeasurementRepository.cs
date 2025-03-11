using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using FlowGuardMonitoring.DAL;
using FlowGuardMonitoring.DAL.Data;
using FlowGuardMonitoring.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Repositories;

    public class MeasurementRepository(FlowGuardMonitoringContext context) : IRepository<Measurement>
    {
        public async Task<List<Measurement>> GetAllAsync()
        {
            return await context.Measurements.ToListAsync();
        }

        public async Task<List<Measurement>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection,
            string searchValue,
            string userId)
        {
            var query = context.Measurements
                .Include(m => m.Sensor)
                .Where(m => m.Sensor.Site.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m =>
                    m.Sensor.Name.Contains(searchValue) ||
                    m.Value.Contains(searchValue));
            }

            // Apply sorting
            switch (sortColumn.ToLower())
            {
                case "sensorname":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.Sensor.Name) : query.OrderByDescending(m => m.Sensor.Name);
                    break;
                case "value":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.Value) : query.OrderByDescending(m => m.Value);
                    break;
                case "type":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.Sensor.Type) : query.OrderByDescending(m => m.Sensor.Type);
                    break;
                default:
                    query = sortDirection == "asc" ? query.OrderBy(m => m.Timestamp) : query.OrderByDescending(m => m.Timestamp);
                    break;
            }

            // Apply pagination
            return await query
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

        public async Task<List<Measurement>> GetAllForSensorAndMonth(int sensorId, int month, int year)
        {
            return await context.Measurements
                .Where(m =>
                    m.SensorId == sensorId &&
                    m.Timestamp.Month == month &&
                    m.Timestamp.Year == year)
                .ToListAsync();
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

        public int GetCount(string userId, string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
            {
                return context.Measurements
                    .Count(m => m.Sensor.Site.UserId == userId);
            }

            return context.Measurements
                .Count(m =>
                    m.Sensor.Site.UserId == userId && (
                    m.Sensor.Name.Contains(searchValue) ||
                    m.Value.Contains(searchValue)));
        }
    }
