using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.DAL.Models;

public class Measurement
{
    [Key]
    public int MeasurementId { get; set; }

    public DateTime Timestamp { get; set; }
    
    public float WaterLevel { get; set; }
    
    public float Temperature { get; set; }
    
    public float pH { get; set; }

    [MaxLength(50)]
    public string Contaminants { get; set; } = "";
    
    public float QualityIndex { get; set; }

    public int SensorId { get; set; }
    public Sensor Sensor { get; set; }
}