using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.DAL.Models;

public class Measurement
{
    [Key]
    public int MeasurementId { get; set; }

    public DateTime Timestamp { get; set; }
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;
    public int SensorId { get; set; }
    public required Sensor Sensor { get; set; }
}