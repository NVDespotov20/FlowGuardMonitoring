using System.Diagnostics;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.WebHost.Models;
using FlowGuardMonitoring.WebHost.Models.Home;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly StatsService statsService;
    public HomeController(ILogger<HomeController> logger, StatsService statsService)
    {
        this.logger = logger;
        this.statsService = statsService;
    }

    [HttpGet("/")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Index()
    {
        IndexViewModel model = new IndexViewModel
        {
            LocationCount = this.statsService.GetLocationsCount(),
            MeasurementCount = this.statsService.GetMeasurementsCount(),
            SensorCount = this.statsService.GetSensorsCount(),
        };
        return this.View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}