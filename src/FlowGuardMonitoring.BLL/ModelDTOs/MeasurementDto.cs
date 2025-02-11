using System;

namespace FlowGuardMonitoring.BLL.ModelDTOs;
public class MeasurementDto
{
    public DateTime Timestamp { get; set; }
    public float WaterLevel { get; set; }
    public float Temperature { get; set; }
    public float pH { get; set; }
    public string Contaminants { get; set; } = string.Empty;
    public float QualityIndex { get; set; }
}
