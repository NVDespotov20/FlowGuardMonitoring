using FlowGuardMonitoring.WebHost.Models.Tables;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FlowGuardMonitoring.WebHost.Models.Management;

public class AddSensorViewModel
{
    public SensorViewModel Sensor { get; set; } = new SensorViewModel();
    [ValidateNever]
    public SiteViewModel? Site { get; set; } = new SiteViewModel();
}