using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Statistics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class StatisticsController : Controller
{
    private readonly MeasurementRepository measurementRepository;
    private readonly IRepository<Sensor> sensorRepository;

    public StatisticsController(MeasurementRepository measurementRepository, IRepository<Sensor> sensorRepository)
    {
        this.measurementRepository = measurementRepository;
        this.sensorRepository = sensorRepository;
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
        return this.View();
    }
}