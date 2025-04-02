using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowGuardMonitoring.WebHost.Models.Statistics;

public class StatisticsRequestModel
{
    [Required]
    [JsonPropertyName("sensorId")]
    public int SensorId { get; set; }
    [Required]
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }
    [Required]
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }
}