using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.WebHost.Models.Tables;

public class MeasurementViewModel
{
    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public float? WaterLevel { get; set; }

    [Required]
    public float? Temperature { get; set; }

    [Required]
    public float? pH { get; set; }

    [MaxLength(50)]
    public string? Contaminants { get; set; } = string.Empty;

    [Required]
    public float? QualityIndex { get; set; }

    [Required]
    public string SensorName { get; set; } = string.Empty;
}