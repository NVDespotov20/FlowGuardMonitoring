using System;
using System.Text.Json.Serialization;

namespace FlowGuardMonitoring.BLL.ModelDTOs;
public class MeasurementDto
{
    [JsonPropertyName("timeStamp")]
    public DateTime TimeStamp { get; set; }
    [JsonPropertyName("waterLevel")]
    public float? WaterLevel { get; set; }
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }
    [JsonPropertyName("pH")]
    public float? pH { get; set; }
    [JsonPropertyName("contaminants")]
    public string? Contaminants { get; set; }
    [JsonPropertyName("qualityIndex")]
    public float? QualityIndex { get; set; }
}
