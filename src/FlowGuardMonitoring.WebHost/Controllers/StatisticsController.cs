using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Statistics;
using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class StatisticsController : Controller
{
    private readonly MeasurementRepository measurementRepository;
    private readonly IRepository<Sensor> sensorRepository;
    private readonly IPaginationService<Measurement> measurementsPagination;
    private readonly ICurrentUser currentUser;

    public StatisticsController(MeasurementRepository measurementRepository, IRepository<Sensor> sensorRepository, ICurrentUser currentUser, IPaginationService<Measurement> measurementsPagination)
    {
        this.measurementRepository = measurementRepository;
        this.sensorRepository = sensorRepository;
        this.currentUser = currentUser;
        this.measurementsPagination = measurementsPagination;
    }

    [HttpPost("/api/statistics/generate")]
    public async Task<IActionResult> GetSensorData([FromBody]StatisticsRequestModel model)
    {
        try
        {
            if (!this.ModelState.IsValid || model.SensorId == 0)
            {
                return this.BadRequest();
            }

            var data = await this.measurementRepository.GetChartElements(model.SensorId, model.StartDate, model.EndDate);

            return new JsonResult(new { success = true, data });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("/statistics/{sensorId?}")]
    public IActionResult SensorStatistics(int sensorId)
    {
        this.ViewBag.SensorId = sensorId;
        return this.View(new PaginatedResult<MeasurementViewModel>());
    }

    [HttpPost("/statistics/{sensorId}/measurements")]
    public async Task<IActionResult> GetMeasurements(int sensorId, [FromBody] DataTablesRequest request)
    {
        int pageNumber = (request.Start / request.Length) + 1;

        var measurements = await this.measurementsPagination.GetPaginatedRecords(
            pageNumber,
            request.Length,
            request.SortColumn,
            request.SortDirection,
            request.SearchValue,
            this.currentUser.UserId,
            sensorId);

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
}