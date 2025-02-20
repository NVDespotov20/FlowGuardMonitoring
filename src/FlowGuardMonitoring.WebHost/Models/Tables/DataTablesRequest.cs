using System.Text.Json.Serialization;

namespace FlowGuardMonitoring.WebHost.Models.Tables;

public class DataTablesRequest
{
    [JsonPropertyName("draw")]
    public int Draw { get; set; }
    [JsonPropertyName("start")]
    public int Start { get; set; }
    [JsonPropertyName("length")]
    public int Length { get; set; }
    [JsonPropertyName("sortColumn")]
    public string SortColumn { get; set; } = string.Empty;
    [JsonPropertyName("sortDirection")]
    public string SortDirection { get; set; } = string.Empty;
    [JsonPropertyName("searchValue")]
    public string SearchValue { get; set; } = string.Empty;
}
