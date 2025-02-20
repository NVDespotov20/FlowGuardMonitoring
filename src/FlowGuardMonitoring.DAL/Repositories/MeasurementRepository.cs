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
        public async Task<List<Measurement>> GetAllAsync()
        {
            return await context.Measurements.ToListAsync();
        }

        public async Task<List<Measurement>> GetPagedAsync(int pageNumber, int pageSize, string sortColumn, string sortDirection, string searchValue)
        {
            // Start with the base query
            var query = context.Measurements
                .Include(m => m.Sensor)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m =>
                    m.Sensor.Name.Contains(searchValue) ||
                    m.Contaminants!.Contains(searchValue) ||
                    m.pH.ToString()!.Contains(searchValue) ||
                    m.QualityIndex.ToString()!.Contains(searchValue) ||
                    m.Temperature.ToString()!.Contains(searchValue) ||
                    m.WaterLevel.ToString()!.Contains(searchValue));
            }

            // Apply sorting
            switch (sortColumn.ToLower())
            {
                case "sensorname":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.Sensor.Name) : query.OrderByDescending(m => m.Sensor.Name);
                    break;
                case "ph":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.pH) : query.OrderByDescending(m => m.pH);
                    break;
                case "qualityindex":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.QualityIndex) : query.OrderByDescending(m => m.QualityIndex);
                    break;
                case "temperature":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.Temperature) : query.OrderByDescending(m => m.Temperature);
                    break;
                case "waterlevel":
                    query = sortDirection == "asc" ? query.OrderBy(m => m.WaterLevel) : query.OrderByDescending(m => m.WaterLevel);
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

        public int GetCount(string searchValue)
        {
            var query = context.Measurements.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m => m.Sensor.Name.Contains(searchValue) ||
                                         (m.Contaminants ?? string.Empty).Contains(searchValue) ||
                                         (m.pH.ToString() ?? string.Empty).Contains(searchValue) ||
                                         (m.WaterLevel.ToString() ?? string.Empty).Contains(searchValue));
            }

            return query.Count();
        }
    }
