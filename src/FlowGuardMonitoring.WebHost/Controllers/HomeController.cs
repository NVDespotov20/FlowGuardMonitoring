using System.Diagnostics;
using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models;
using FlowGuardMonitoring.WebHost.Models.Home;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly StatisticsService statisticsService;
    private readonly SensorRepository sensorRepository;
    private readonly SiteRepository siteRepository;
    private readonly ICurrentUser currentUser;
    public HomeController(ILogger<HomeController> logger, StatisticsService statisticsService, ICurrentUser currentUser, SensorRepository sensorRepository, SiteRepository siteRepository)
    {
        this.logger = logger;
        this.statisticsService = statisticsService;
        this.currentUser = currentUser;
        this.sensorRepository = sensorRepository;
        this.siteRepository = siteRepository;
    }

    [HttpGet("/")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Index()
    {
        IndexViewModel model = new IndexViewModel
        {
            LocationCount = this.statisticsService.GetLocationsCount(this.currentUser.UserId),
            MeasurementCount = this.statisticsService.GetMeasurementsCount(this.currentUser.UserId),
            SensorCount = this.statisticsService.GetSensorsCount(this.currentUser.UserId),
            Locations = (await this.siteRepository.GetByUserId(this.currentUser.UserId))
                .Select(l => new LocationViewModel
                {
                    SiteId = l.SiteId,
                    Name = l.Name,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                }).ToList(),
        };
        return this.View(model);
    }

    [HttpGet("/api/locations/{siteId}/sensors")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetSensorsForLocation(int siteId)
    {
        var sensors = (await this.sensorRepository.GetBySiteIdAsync(siteId))
            .Select(s => new SensorViewModel
            {
                SensorId = s.SensorId,
                Name = s.Name,
                Latitude = s.Latitude ?? 0,
                Longitude = s.Longitude ?? 0,
            });

        return this.Json(sensors);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}