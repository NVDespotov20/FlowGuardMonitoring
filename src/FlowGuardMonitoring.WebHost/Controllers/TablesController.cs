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
    public TablesController(
        IPaginationService<Sensor> sensorPagination,
        IPaginationService<Site> sitesPagination,
        IPaginationService<Measurement> measurementsPagination)
    {
        this.sensorPagination = sensorPagination;
        this.sitesPagination = sitesPagination;
        this.measurementsPagination = measurementsPagination;
    }

    public async Task<ActionResult> Sensors(int page = 1, int pageSize = 10)
    {
        var sensors = await this.sensorPagination.GetPaginatedRecords(page, pageSize);
        var viewModel = new PaginatedResult<SensorViewModel>()
        {
            Records = sensors.Records.Select(s => new SensorViewModel
            {
                Name = s.Name,
                InstallationDate = s.InstallationDate,
                IsActive = s.IsActive,
                SiteName = s.Site.Name,
                Type = s.Type,
            }).ToList(),
            PageNumber = sensors.PageNumber,
            PageSize = sensors.PageSize,
            TotalRecords = sensors.TotalRecords,
            TotalPages = sensors.TotalPages,
        };
        return this.View(viewModel);
    }

    public async Task<ActionResult> Sites(int page = 1, int pageSize = 10)
    {
        var sites = await this.sitesPagination.GetPaginatedRecords(page, pageSize);
        var viewModel = new PaginatedResult<SiteViewModel>()
        {
            Records = sites.Records.Select(s => new SiteViewModel
            {
                Name = s.Name,
                Description = s.Description,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
            }).ToList(),
            PageNumber = sites.PageNumber,
            PageSize = sites.PageSize,
            TotalRecords = sites.TotalRecords,
            TotalPages = sites.TotalPages,
        };
        return this.View(viewModel);
    }

    public async Task<ActionResult> Measurements(int page = 1, int pageSize = 10)
    {
        var measurements = await this.measurementsPagination.GetPaginatedRecords(page, pageSize);
        var viewModel = new PaginatedResult<MeasurementViewModel>()
        {
            Records = measurements.Records.Select(m => new MeasurementViewModel
            {
                Timestamp = m.Timestamp,
                Contaminants = m.Contaminants,
                pH = m.pH,
                QualityIndex = m.QualityIndex,
                SensorName = m.Sensor.Name,
                Temperature = m.Temperature,
                WaterLevel = m.WaterLevel,
            }).ToList(),
            PageNumber = measurements.PageNumber,
            PageSize = measurements.PageSize,
            TotalRecords = measurements.TotalRecords,
            TotalPages = measurements.TotalPages,
        };
        return this.View(viewModel);
    }
}
