using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.WebHost.Models.Tables;

public class MeasurementViewModel
{
    [Required]
    public string SensorName { get; set; } = string.Empty;
    [Required]
    public string Timestamp { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;
    [Required]
    public string Value { get; set; } = string.Empty;
}