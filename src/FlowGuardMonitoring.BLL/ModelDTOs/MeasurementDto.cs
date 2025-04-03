using System;
using System.Text.Json.Serialization;

namespace FlowGuardMonitoring.BLL.ModelDTOs;
public class MeasurementDto
{
    [JsonPropertyName("timeStamp")]
    public DateTime TimeStamp { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    [JsonPropertyName("rawValue")]
    public double RawValue { get; set; }
}
