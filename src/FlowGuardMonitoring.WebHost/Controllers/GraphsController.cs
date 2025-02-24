using FlowGuardMonitoring.DAL.Models;
using FlowGuardMonitoring.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FlowGuardMonitoring.WebHost.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class GraphsController : ControllerBase
{
    private readonly MeasurementRepository measurementRepository;

    public GraphsController(MeasurementRepository measurementRepository)
    {
        this.measurementRepository = measurementRepository;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyDataForSensor(int sensorId, int month, int year)
    {
        /*List<Measurement> readings = await this.measurementRepository.GetAllForSensorAndMonth(sensorId, month, year);
        var groupedData = readings.GroupBy(r => r.Timestamp.Day)
            .Select(g => new
            {
                Day = g.Key,
                Average = g.Average(r => r),
                Min = g.Min(r => r.Value),
                Max = g.Max(r => r.Value),
            })
            .ToDictionary(x => x.Day);

        // Get the number of days in the month
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

        // Prepare a result array including every day of the month
        var result = new List<object>();
        for (int day = 1; day <= daysInMonth; day++)
        {
            if (groupedData.ContainsKey(day))
            {
                var data = groupedData[day];
                result.Add(new { Day = day, data.Average, data.Min, data.Max });
            }
            else
            {
                // No data for this day, return nulls or defaults as needed
                result.Add(new { Day = day, Average = (double?)null, Min = (double?)null, Max = (double?)null });
            }
        }
        */

        return this.Ok();
    }
}