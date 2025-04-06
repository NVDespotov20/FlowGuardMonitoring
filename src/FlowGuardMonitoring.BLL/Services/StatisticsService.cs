using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Microsoft.IdentityModel.Tokens;
using Resend;

namespace FlowGuardMonitoring.BLL.Services;

public class StatisticsService
{
    private readonly IRepository<Sensor> sensorRepository;
    private readonly MeasurementRepository measurementRepository;
    private readonly IRepository<Site> siteRepository;

    public StatisticsService(IRepository<Sensor> sensorRepository, MeasurementRepository measurementRepository, IRepository<Site> siteRepository)
    {
        this.sensorRepository = sensorRepository;
        this.measurementRepository = measurementRepository;
        this.siteRepository = siteRepository;
    }

    public int GetSensorsCount(string userId)
    {
        return this.sensorRepository.GetCount(userId);
    }

    public int GetMeasurementsCount(string userId)
    {
        return this.measurementRepository.GetCount(userId, string.Empty);
    }

    public MeasurementStatistics CalculateStatistics(List<MeasurementExportViewModel> data)
    {
        return new MeasurementStatistics
        {
            Latest = (float)data.Last().RawValue,
            Average = (float)data.Average(m => m.RawValue),
            Min = (float)data.Min(m => m.RawValue),
            Max = (float)data.Max(m => m.RawValue),
        };
    }

    public async Task<List<Measurement>> GetMeasurementsGrouped(string userId, int sensorId, string timeframe)
    {
        if (userId.IsNullOrEmpty())
        {
            return new List<Measurement>();
        }

        if (sensorId <= 0)
        {
            return new List<Measurement>();
        }

        _ = await this.measurementRepository.GetByIdAsync(sensorId);
        return new List<Measurement>();
    }

    public int GetLocationsCount(string userId)
    {
        return this.siteRepository.GetCount(userId);
    }
}