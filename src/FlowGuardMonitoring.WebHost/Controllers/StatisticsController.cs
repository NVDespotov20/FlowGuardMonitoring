using FlowGuardMonitoring.BLL.Contracts;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Services;
using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using FlowGuardMonitoring.WebHost.Models.Statistics;
using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.View;

namespace FlowGuardMonitoring.WebHost.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[Route("/statistics")]
public class StatisticsController : Controller
{
    private readonly MeasurementRepository measurementRepository;
    private readonly IRepository<Sensor> sensorRepository;
    private readonly IPaginationService<Measurement> measurementsPagination;
    private readonly ExportService exportService;
    private readonly ICurrentUser currentUser;
    private readonly ILogger<StatisticsController> logger;

    public StatisticsController(
        MeasurementRepository measurementRepository,
        IRepository<Sensor> sensorRepository,
        ICurrentUser currentUser,
        IPaginationService<Measurement> measurementsPagination,
        ExportService exportService,
        ILogger<StatisticsController> logger)
    {
        this.measurementRepository = measurementRepository;
        this.sensorRepository = sensorRepository;
        this.currentUser = currentUser;
        this.measurementsPagination = measurementsPagination;
        this.exportService = exportService;
        this.logger = logger;
    }

    [HttpPost("/api/statistics/generate")]
    public async Task<IActionResult> GetSensorData([FromBody] StatisticsRequestModel model)
    {
        try
        {
            if (!this.ModelState.IsValid || model.SensorId == 0)
            {
                return this.BadRequest();
            }

            var data = await this.measurementRepository.GetChartElements(
                model.SensorId,
                model.StartDate,
                model.EndDate);

            return new JsonResult(new { success = true, data });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{sensorId?}")]
    public async Task<IActionResult> SensorStatistics(int sensorId)
    {
        this.ViewBag.SensorId = sensorId;
        this.ViewBag.SensorName = (await this.sensorRepository.GetByIdAsync(sensorId))?.Name ?? string.Empty;
        return this.View(new PaginatedResult<MeasurementViewModel>());
    }

    [HttpPost("{sensorId}/measurements")]
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

    [HttpGet("{sensorId}/export/pdf")]
    public async Task<IActionResult> ExportPdf(int sensorId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Get sensor details for the report title
            var sensor = await this.sensorRepository.GetByIdAsync(sensorId);
            if (sensor == null)
            {
                return this.NotFound();
            }

            // Get measurements data
            var result = await this.measurementRepository.GetChartElements(
                sensorId,
                startDate,
                endDate);

            var processedData = this.ProcessDataForExport(result);

            var pdfData = this.exportService.GeneratePdf(sensor.Name, startDate, endDate, processedData);

            return this.File(
                pdfData,
                "application/pdf",
                $"Sensor_{sensor.Name}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error exporting PDF for sensor {SensorId}", sensorId);
            return this.StatusCode(500, "An error occurred while generating the PDF export.");
        }
    }

    [HttpGet("{sensorId}/export/excel")]
    public async Task<IActionResult> ExportExcel(int sensorId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Get sensor details for the spreadsheet title
            var sensor = await this.sensorRepository.GetByIdAsync(sensorId);
            if (sensor == null)
            {
                return this.NotFound();
            }

            // Get measurements data
            var result = await this.measurementRepository.GetChartElements(
                sensorId,
                startDate,
                endDate);

            // Process the data
            var processedData = this.ProcessDataForExport(result);

            // Generate Excel file
            var excelData = this.exportService.GenerateExcel(sensor.Name, startDate, endDate, processedData);

            return this.File(
                excelData,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sensor_{sensor.Name}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error exporting Excel for sensor {SensorId}", sensorId);
            return this.StatusCode(500, "An error occurred while generating the Excel export.");
        }
    }

    private List<MeasurementExportViewModel> ProcessDataForExport(List<Measurement> data)
    {
        // Extract numeric values from measurements
        var regex = new System.Text.RegularExpressions.Regex(@"-?(\d+(\.\d+)?|\.\d+)");

        return data
            .Select(m => new MeasurementExportViewModel
            {
                Timestamp = m.Timestamp,
                Value = m.Value,
                RawValue = m.RawValue,
            })
            .OrderBy(m => m.Timestamp)
            .ToList();
    }
}