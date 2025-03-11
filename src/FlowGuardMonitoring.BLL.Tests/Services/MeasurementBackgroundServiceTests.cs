using System.Net;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace FlowGuardMonitoring.BLL.Tests.Services;

public class MeasurementBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ServiceStarts_LogsStartMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MeasurementBackgroundService>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();

        var service = new MeasurementBackgroundService(mockLogger.Object, mockServiceScopeFactory.Object, mockHttpClientFactory.Object);
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // Ensure the service runs for a short time

        // Act
        await service.StartAsync(cancellationTokenSource.Token);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v!.ToString().Contains("MeasurementBackgroundService is starting.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    [Fact]
    public async Task FetchAndSaveMeasurementsAsync_NoSensorsFound_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MeasurementBackgroundService>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockSensorRepo = new Mock<IRepository<Sensor>>();
        var mockMeasurementRepo = new Mock<IRepository<Measurement>>();
        mockSensorRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Sensor>());
        mockServiceProvider.Setup(x => x.GetService(typeof(IRepository<Sensor>))).Returns(mockSensorRepo.Object);
        mockServiceProvider.Setup(x => x.GetService(typeof(IRepository<Measurement>))).Returns(mockMeasurementRepo.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var service = new MeasurementBackgroundService(mockLogger.Object, mockServiceScopeFactory.Object, mockHttpClientFactory.Object);

        // Act
        await service.FetchAndSaveMeasurementsAsync(CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No sensors found in the database.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    [Fact]
    public async Task FetchAndSaveMeasurementsAsync_SuccessfullySavesMeasurement()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MeasurementBackgroundService>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var sensor = new Sensor
        {
            SensorId = 1,
            Site = null,
        };

        mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockSensorRepo = new Mock<IRepository<Sensor>>();
        mockSensorRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Sensor> { sensor });
        mockServiceProvider.Setup(x => x.GetService(typeof(IRepository<Sensor>))).Returns(mockSensorRepo.Object);

        var mockMeasurementRepo = new Mock<IRepository<Measurement>>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IRepository<Measurement>))).Returns(mockMeasurementRepo.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When("")
            .Respond("application/json", "{\"Value\": 100, \"TimeStamp\": \"2023-01-01T00:00:00Z\"}");
        var httpClient = mockHttpMessageHandler.ToHttpClient();
        mockHttpClientFactory.Setup(h => h.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new MeasurementBackgroundService(mockLogger.Object, mockServiceScopeFactory.Object, mockHttpClientFactory.Object);

        // Act
        await service.FetchAndSaveMeasurementsAsync(CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetched and saved measurement for sensor")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    [Fact]
    public async Task FetchAndSaveMeasurementsAsync_ApiRequestFails_LogsError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MeasurementBackgroundService>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        var sensor = new Sensor
        {
            SensorId = 1,
            Site = null,
        };

        mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockSensorRepo = new Mock<IRepository<Sensor>>();
        var mockMeasurementRepo = new Mock<IRepository<Measurement>>();
        mockSensorRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Sensor> { sensor });
        mockServiceProvider.Setup(x => x.GetService(typeof(IRepository<Sensor>))).Returns(mockSensorRepo.Object);
        mockServiceProvider.Setup(x => x.GetService(typeof(IRepository<Measurement>))).Returns(mockMeasurementRepo.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        mockHttpMessageHandler.When($"http://127.0.0.1:5000/sensor/{sensor.SensorId}/measurement")
            .Respond(HttpStatusCode.BadRequest);
        var httpClient = mockHttpMessageHandler.ToHttpClient();
        mockHttpClientFactory.Setup(h => h.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new MeasurementBackgroundService(mockLogger.Object, mockServiceScopeFactory.Object, mockHttpClientFactory.Object);

        // Act
        await service.FetchAndSaveMeasurementsAsync(CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to fetch measurement for sensor")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}