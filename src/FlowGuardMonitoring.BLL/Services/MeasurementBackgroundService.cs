using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlowGuardMonitoring.BLL.Contracts;
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

    internal async Task FetchAndSaveMeasurementsAsync(CancellationToken stoppingToken)
    {
        using var scope = this.serviceScopeFactory.CreateScope();
        var sensorRepository = scope.ServiceProvider.GetRequiredService<IRepository<Sensor>>();
        var measurementRepository = scope.ServiceProvider.GetRequiredService<IRepository<Measurement>>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();
        var client = this.httpClientFactory.CreateClient("MeasurementApi");

        List<Sensor> sensors = await sensorRepository.GetAllAsync();

        if (!sensors.Any())
        {
            this.logger.LogWarning("No sensors found in the database.");
            return;
        }

        foreach (var sensor in sensors)
        {
            try
            {
                var response = await client.GetAsync(
                    $"http://127.0.0.1:5000/sensor/{sensor.SensorId}/measurement",
                    stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(stoppingToken);
                    var measurementDto = JsonSerializer.Deserialize<MeasurementDto>(json);
                    if (measurementDto != null)
                    {
                        var measurement = this.MapDtoToMeasurement(measurementDto, sensor.SensorId);
                        measurement.Sensor = (await sensorRepository.GetByIdAsync(sensor.SensorId))!;

                        await measurementRepository.AddAsync(measurement);

                        var notification = await this.HandleNotifications(measurement);
                        if (notification != null)
                        {
                            await notificationRepository.AddAsync(notification);
                        }

                        this.logger.LogInformation($"Fetched and saved measurement for sensor {sensor.SensorId}.");
                    }
                }
                else
                {
                    await notificationRepository.AddAsync(new Notification
                    {
                        Title = $"Measurement for sensor {sensor.SensorId} failed",
                        Description =
                            $"The sensor {sensor.Name} refused to talk to the platform. This could be a one time issue, however further investigation is suggested.",
                        Type = "Info",
                        Date = DateTime.Now,
                        UserId = sensor.Site.UserId,
                    });
                    this.logger.LogError(
                        $"Failed to fetch measurement for sensor {sensor.SensorId}. Status code: {response.StatusCode}");
                }
            }
            catch
            {
                    await notificationRepository.AddAsync(new Notification
                    {
                        Title = $"Sensor not accessible",
                        Description =
                            $"Sensor {sensor.Name} is not accessible via the API. The sensor might be disconnected.",
                        Type = "Error",
                        Date = DateTime.Now,
                        UserId = sensor.Site.UserId,
                    });
                    this.logger.LogError(
                        $"Failed to access the API. Is the API up?");
            }
        }
    }

    internal async Task<Notification?> HandleNotifications(Measurement measurement)
    {
        switch (measurement.Sensor.Type)
        {
        case SensorType.Temperature:
            if (double.TryParse(measurement.Value.Substring(0, measurement.Value.Length - 2), out var temperature) &&
                (temperature < 0 || temperature > 32))
            {
                return new Notification
                {
                    NotificationId = 0,
                    Title = "Abnormal Temperature",
                    Description = $"The temperature measured by temperature sensor located on {measurement.Sensor.Site.Name} has detected an abnormal temperature of {measurement.Value}.",
                    Type = "Warning",
                    Date = DateTime.Now,
                    UserId = measurement.Sensor.Site.UserId,
                };
            }

            break;
        case SensorType.Contaminants:
            break;
        case SensorType.Level:
            if (int.TryParse(measurement.Value.Substring(0, measurement.Value.Length - 1), out var level) &&
                (level < 1 || level > 8))
            {
                return new Notification
                {
                    NotificationId = 0,
                    Title = "Abnormal Water Level",
                    Description = $"The water level at {measurement.Sensor.Site.Name} was measured to be abnormal: {measurement.Value}.",
                    Type = "Warning",
                    Date = DateTime.Now,
                    UserId = measurement.Sensor.Site.UserId,
                };
            }

            break;
        case SensorType.Quality:
            if (int.TryParse(measurement.Value.Substring(0, measurement.Value.Length - 1), out var quality) &&
                 quality < 70)
            {
                return new Notification
                {
                    NotificationId = 0,
                    Title = "Abnormal Water Quality",
                    Description = $"The water quality on {measurement.Sensor.Site.Name} has been detected as dangerously low with a rating of {measurement.Value}.",
                    Type = "Warning",
                    Date = DateTime.Now,
                    UserId = measurement.Sensor.Site.UserId,
                };
            }

            break;
        case SensorType.Ph:
            if (int.TryParse(measurement.Value, out var pH) &&
                (pH < 6 || pH > 8))
            {
                return new Notification
                {
                    NotificationId = 0,
                    Title = "Abnormal PH Level",
                    Description = $"The water PH level at {measurement.Sensor.Site.Name} was measured to be abnormal: {measurement.Value}.",
                    Type = "Warning",
                    Date = DateTime.Now,
                    UserId = measurement.Sensor.Site.UserId,
                };
            }

            break;
        }

        return null;
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

    private Measurement MapDtoToMeasurement(MeasurementDto dto, int sensorId)
    {
        return new Measurement
        {
            Timestamp = dto.TimeStamp,
            Value = dto.Value,
            RawValue = dto.RawValue,
            SensorId = sensorId,
            Sensor = null!,
        };
    }
}
