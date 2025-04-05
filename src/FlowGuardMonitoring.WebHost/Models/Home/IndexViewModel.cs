namespace FlowGuardMonitoring.WebHost.Models.Home;

public class IndexViewModel
{
    public int MeasurementCount { get; set; }
    public int SensorCount { get; set; }
    public int LocationCount { get; set; }
    public List<LocationViewModel> Locations { get; set; } = new List<LocationViewModel>();
}