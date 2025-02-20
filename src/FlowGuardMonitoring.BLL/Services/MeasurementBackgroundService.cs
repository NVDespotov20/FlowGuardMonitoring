using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlowGuardMonitoring.BLL.ModelDTOs;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlowGuardMonitoring.BLL.Services;

public class MeasurementBackgroundService : BackgroundService
{
    private readonly ILogger<MeasurementBackgroundService> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly TimeSpan interval = TimeSpan.FromMinutes(1);

    public MeasurementBackgroundService(
        ILogger<MeasurementBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("MeasurementBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.FetchAndSaveMeasurementsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while fetching and saving measurements.");
            }

            await Task.Delay(this.interval, stoppingToken);
        }

        this.logger.LogInformation("MeasurementBackgroundService is stopping.");
    }

    private async Task FetchAndSaveMeasurementsAsync(CancellationToken stoppingToken)
    {
        using var scope = this.serviceScopeFactory.CreateScope();
        var sensorRepository = scope.ServiceProvider.GetRequiredService<IRepository<Sensor>>();
        var measurementRepository = scope.ServiceProvider.GetRequiredService<IRepository<Measurement>>();

        var client = this.httpClientFactory.CreateClient("MeasurementApi");

        List<Sensor> sensors = await sensorRepository.GetAllAsync();

        if (sensors == null || !sensors.Any())
        {
            this.logger.LogWarning("No sensors found in the database.");
            return;
        }

        foreach (var sensor in sensors)
        {
            var response = await client.GetAsync($"http://127.0.0.1:5000/sensor/{sensor.SensorId}/measurement", stoppingToken);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync(stoppingToken);
                var measurementDto = JsonSerializer.Deserialize<MeasurementDto>(jsonResponse);

                if (measurementDto != null)
                {
                    var measurement = this.MapDtoToMeasurement(measurementDto, sensor.SensorId);
                    measurement.Sensor = (await sensorRepository.GetByIdAsync(sensor.SensorId))!;

                    await measurementRepository.AddAsync(measurement);

                    this.logger.LogInformation($"Fetched and saved measurement for sensor {sensor.SensorId}.");
                }
            }
            else
            {
                this.logger.LogError($"Failed to fetch measurement for sensor {sensor.SensorId}. Status code: {response.StatusCode}");
            }
        }
    }

    private Measurement MapDtoToMeasurement(MeasurementDto dto, int sensorId)
    {
        return new Measurement
        {
            Timestamp = dto.TimeStamp,
            WaterLevel = dto.WaterLevel,
            Temperature = dto.Temperature,
            pH = dto.pH,
            Contaminants = dto.Contaminants,
            QualityIndex = dto.QualityIndex,
            SensorId = sensorId,
            Sensor = null!,
        };
    }
}
