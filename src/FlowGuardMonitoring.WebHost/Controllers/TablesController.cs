using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class TablesController : Controller
{
    private readonly IPaginationService<Sensor> sensorPagination;
    private readonly IPaginationService<Site> sitesPagination;
    private readonly IPaginationService<Measurement> measurementsPagination;
    private readonly ICurrentUser currentUser;
    public TablesController(
        IPaginationService<Sensor> sensorPagination,
        IPaginationService<Site> sitesPagination,
        IPaginationService<Measurement> measurementsPagination,
        ICurrentUser currentUser)
    {
        this.sensorPagination = sensorPagination;
        this.sitesPagination = sitesPagination;
        this.measurementsPagination = measurementsPagination;
        this.currentUser = currentUser;
    }

    public async Task<ActionResult> Sensors()
    {
        return this.View(new PaginatedResult<SensorViewModel>());
    }

    public async Task<ActionResult> Sites()
    {
        return this.View(new PaginatedResult<SiteViewModel>());
    }

    public async Task<ActionResult> Measurements()
    {
        return this.View(new PaginatedResult<MeasurementViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> GetMeasurements([FromBody] DataTablesRequest request)
    {
        int pageNumber = (request.Start / request.Length) + 1;

        var measurements = await this.measurementsPagination.GetPaginatedRecords(
            pageNumber,
            request.Length,
            request.SortColumn,
            request.SortDirection,
            request.SearchValue,
            this.currentUser.UserId);

        var viewModel = measurements.Records.Select(m => new MeasurementViewModel
        {
            SensorName = m.Sensor.Name,
            Timestamp = m.Timestamp.ToString("dd-MM-yyyy HH:mm"),
            Type = m.Sensor.Type.ToString(),
            Value = m.Value,
        }).ToList();

        var jsonData = new
        {
            draw = request.Draw,
            recordsFiltered = measurements.TotalRecords,
            recordsTotal = measurements.TotalRecords,
            data = viewModel,
        };

        return this.Ok(jsonData);
    }

    [HttpPost]
    public async Task<IActionResult> GetSensors([FromBody] DataTablesRequest request)
    {
        int pageNumber = (request.Start / request.Length) + 1;

        var sensors = await this.sensorPagination.GetPaginatedRecords(
            pageNumber,
            request.Length,
            request.SortColumn,
            request.SortDirection,
            request.SearchValue,
            this.currentUser.UserId);

        var viewModel = sensors.Records.Select(s => new
        {
            Name = s.Name,
            Type = s.Type.ToString(),
            InstallationDate = s.InstallationDate,
            IsActive = s.IsActive,
            SiteName = s.Site.Name,
            SensorId = s.SensorId,
        }).ToList();

        var jsonData = new
        {
            draw = request.Draw,
            recordsFiltered = sensors.TotalRecords,
            recordsTotal = sensors.TotalRecords,
            data = viewModel,
        };

        return this.Ok(jsonData);
    }

    [HttpPost]
    public async Task<IActionResult> GetSites([FromBody] DataTablesRequest request)
    {
        int pageNumber = (request.Start / request.Length) + 1;

        var sites = await this.sitesPagination.GetPaginatedRecords(
            pageNumber,
            request.Length,
            request.SortColumn,
            request.SortDirection,
            request.SearchValue,
            this.currentUser.UserId);

        var viewModel = sites.Records.Select(s => new SiteViewModel
        {
            Name = s.Name,
            Description = s.Description,
            Latitude = s.Latitude,
            Longitude = s.Longitude,
        }).ToList();

        var jsonData = new
        {
            draw = request.Draw,
            recordsFiltered = sites.TotalRecords,
            recordsTotal = sites.TotalRecords,
            data = viewModel,
        };

        return this.Ok(jsonData);
    }
}
