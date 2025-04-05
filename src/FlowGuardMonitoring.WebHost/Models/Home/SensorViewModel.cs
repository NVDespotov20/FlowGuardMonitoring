namespace FlowGuardMonitoring.WebHost.Models.Home;

public class SensorViewModel
{
    public int SensorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}