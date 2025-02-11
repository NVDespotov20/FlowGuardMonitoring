using System.Security.Claims;
using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Management;
using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class ManagementController : Controller
{
    private readonly IRepository<Sensor> sensorRepository;
    private readonly IRepository<Site> siteRepository;
    private readonly ICurrentUser currentUser;
    private readonly UserManager<User> userManager;

    public ManagementController(IRepository<Sensor> sensorRepository, IRepository<Site> siteRepository, ICurrentUser currentUser, UserManager<User> userManager)
    {
        this.sensorRepository = sensorRepository;
        this.siteRepository = siteRepository;
        this.currentUser = currentUser;
        this.userManager = userManager;
    }

    public IActionResult Manage()
    {
        return this.View("AddSensor");
    }

    public async Task<IActionResult> AddSensor()
    {
        // Debug: inspect the claims from HttpContext
        var claims = this.HttpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        // You could log these or temporarily output them to the debug console.
        System.Diagnostics.Debug.WriteLine("User Claims: " + string.Join(", ", claims.Select(c => $"{c.Type}: {c.Value}")));

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
                string.IsNullOrWhiteSpace(model.Site.Name) ||
                string.IsNullOrWhiteSpace(model.Site.Description))
            {
                this.ModelState.AddModelError("Site", "Please provide complete details for the new site.");
                await this.PopulateSites();
                return this.View(model);
            }

            var newSiteEntity = new Site
            {
                Name = model.Site.Name,
                Description = model.Site.Description,
                Latitude = model.Site.Latitude,
                Longitude = model.Site.Longitude,
                UserId = this.currentUser.UserId,
                User = (await this.userManager.GetUserAsync(this.currentUser.User!))!,
            };

            await this.siteRepository.AddAsync(newSiteEntity);

            model.Sensor.SiteName = newSiteEntity.SiteId.ToString();
        }

        if (!int.TryParse(model.Sensor.SiteName, out int siteId))
        {
            this.ModelState.AddModelError("Sensor.SiteName", "Please select a valid site.");
            await this.PopulateSites();
            return this.View(model);
        }

        var sensorEntity = new Sensor
        {
            Name = model.Sensor.Name,
            Type = model.Sensor.Type,
            InstallationDate = model.Sensor.InstallationDate,
            IsActive = model.Sensor.IsActive,
            SiteId = siteId,
            Site = (await this.siteRepository.GetByIdAsync(siteId))!,
        };

        await this.sensorRepository.AddAsync(sensorEntity);

        return this.RedirectToAction("Index", "Home");
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