using System.Diagnostics;
using FlowGuardMonitoring.BLL.Contracts;
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
    private readonly StatisticsService statisticsService;
    private readonly ICurrentUser currentUser;
    public HomeController(ILogger<HomeController> logger, StatisticsService statisticsService, ICurrentUser currentUser)
    {
        this.logger = logger;
        this.statisticsService = statisticsService;
        this.currentUser = currentUser;
    }

    [HttpGet("/")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Index()
    {
        IndexViewModel model = new IndexViewModel
        {
            LocationCount = this.statisticsService.GetLocationsCount(this.currentUser.UserId),
            MeasurementCount = this.statisticsService.GetMeasurementsCount(this.currentUser.UserId),
            SensorCount = this.statisticsService.GetSensorsCount(this.currentUser.UserId),
        };
        return this.View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
    }
}