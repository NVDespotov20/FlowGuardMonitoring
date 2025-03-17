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
        return this.View(new AddSensorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSensor(AddSensorViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            await this.PopulateSites();
            return this.View(model);
        }

        if (model.Sensor.SiteName == "new")
        {
            if (model.Site == null ||
                model.Site.Name.IsNullOrEmpty() ||
                model.Site.Description.IsNullOrEmpty())
            {
                this.ModelState.AddModelError("Site", "Please provide complete details for the new site.");
                await this.PopulateSites();
                return this.View(model);
            }

            var site = new Site
            {
                Name = model.Site.Name,
                Description = model.Site.Description,
                Latitude = model.Site.Latitude,
                Longitude = model.Site.Longitude,
                UserId = this.currentUser.UserId,
            };
            await this.siteRepository.AddAsync(site);
            model.Sensor.SiteName = site.SiteId.ToString();
        }

        if (!int.TryParse(model.Sensor.SiteName, out int siteId))
        {
            this.ModelState.AddModelError("Sensor.SiteName", "Please select a valid site.");
            await this.PopulateSites();
            return this.View(model);
        }

        var sensor = new Sensor
        {
            Name = model.Sensor.Name,
            Type = model.Sensor.Type,
            InstallationDate = model.Sensor.InstallationDate,
            IsActive = model.Sensor.IsActive,
            SiteId = siteId,
            Site = (await this.siteRepository.GetByIdAsync(siteId))!,
        };
        await this.sensorRepository.AddAsync(sensor);
        return this.RedirectToAction("Sensors", "Tables");
    }

    [HttpGet]
    public IActionResult AddSite()
    {
        return this.View();
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
        var sites = new List<SelectListItem>
        {
            new SelectListItem { Value = string.Empty, Text = "-- Select Site --" },
            new() { Value = "new", Text = "Create New Site" },
        };

        var existingSites = (await this.siteRepository.GetAllAsync()).Select(s => new SelectListItem
        {
            Value = s.SiteId.ToString(),
            Text = s.Name,
        }).ToList();
        sites.AddRange(existingSites);

        this.ViewBag.Sites = sites;
    }
}