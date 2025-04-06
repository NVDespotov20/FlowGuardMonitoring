using System;

namespace FlowGuardMonitoring.BLL.Models;

public class MeasurementExportViewModel
{
    public DateTime Timestamp { get; set; }
    public string Value { get; set; } = string.Empty;
    public double RawValue { get; set; }
}