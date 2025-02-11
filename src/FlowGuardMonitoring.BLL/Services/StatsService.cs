using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Resend;

namespace FlowGuardMonitoring.BLL.Services;

public class StatsService
{
    private readonly IRepository<Sensor> sensorRepository;
    private readonly IRepository<Measurement> measurementRepository;
    private readonly IRepository<Site> siteRepository;

    public StatsService(IRepository<Sensor> sensorRepository, IRepository<Measurement> measurementRepository, IRepository<Site> siteRepository)
    {
        this.sensorRepository = sensorRepository;
        this.measurementRepository = measurementRepository;
        this.siteRepository = siteRepository;
    }

    public int GetSensorsCount()
    {
        return this.sensorRepository.GetCount();
    }

    public int GetMeasurementsCount()
    {
        return this.measurementRepository.GetCount();
    }

    public int GetLocationsCount()
    {
        return this.siteRepository.GetCount();
    }
}