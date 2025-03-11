using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Resend;

namespace FlowGuardMonitoring.BLL.Services;

public class StatisticsService
{
    private readonly IRepository<Sensor> sensorRepository;
    private readonly IRepository<Measurement> measurementRepository;
    private readonly IRepository<Site> siteRepository;

    public StatisticsService(IRepository<Sensor> sensorRepository, IRepository<Measurement> measurementRepository, IRepository<Site> siteRepository)
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
        return this.measurementRepository.GetCount(userId);
    }

    public int GetLocationsCount(string userId)
    {
        return this.siteRepository.GetCount(userId);
    }
}