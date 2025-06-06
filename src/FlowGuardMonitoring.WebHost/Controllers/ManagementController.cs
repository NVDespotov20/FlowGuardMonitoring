using System.Security.Claims;
using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Management;
using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class ManagementController : Controller
{
    private readonly IRepository<Sensor> sensorRepository;
    private readonly IRepository<Site> siteRepository;
    private readonly ICurrentUser currentUser;

    public ManagementController(IRepository<Sensor> sensorRepository, IRepository<Site> siteRepository, ICurrentUser currentUser)
    {
        this.sensorRepository = sensorRepository;
        this.siteRepository = siteRepository;
        this.currentUser = currentUser;
    }

    public IActionResult Manage()
    {
        return this.View("AddSensor");
    }

    public async Task<IActionResult> AddSensor()
    {
        await this.PopulateSites();
        return this.View(new SensorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSensor(SensorViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            await this.PopulateSites();
            return this.View(model);
        }

        if (!int.TryParse(model.SiteName, out int siteId))
        {
            this.ModelState.AddModelError("Sensor.SiteName", "Please select a valid location.");
            await this.PopulateSites();
            return this.View(model);
        }

        var sensor = new Sensor
        {
            Name = model.Name,
            Type = model.Type,
            InstallationDate = model.InstallationDate,
            IsActive = model.IsActive,
            SerialNumber = model.SerialNumber,
            Manufacturer = model.Manufacturer,
            ModelNumber = model.ModelNumber,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            SiteId = siteId,
            Site = (await this.siteRepository.GetByIdAsync(siteId))!,
        };

        await this.sensorRepository.AddAsync(sensor);
        return this.RedirectToAction("Sensors", "Tables");
    }

    [HttpGet]
    public IActionResult AddSite()
    {
        return this.View(new SiteViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSite(SiteViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var site = new Site
        {
            Name = model.Name,
            Description = model.Description,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            UserId = this.currentUser.UserId,
        };

        await this.siteRepository.AddAsync(site);
        return this.RedirectToAction("Sites", "Tables");
    }

    private async Task PopulateSites()
    {
        var sites = new List<SiteSelectListItem>
        {
            new()
            {
                Value = string.Empty,
                Text = "-- Select Site --",
                Latitude = 0,
                Longitude = 0,
            },
        };

        var existingSites = (await this.siteRepository.GetAllAsync()).Select(s => new SiteSelectListItem
        {
            Value = s.SiteId.ToString(),
            Text = s.Name,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
        }).ToList();
        sites.AddRange(existingSites);

        this.ViewBag.Sites = sites;
    }
}