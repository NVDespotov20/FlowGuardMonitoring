using System.ComponentModel.DataAnnotations;
using FlowGuardMonitoring.DAL.Models;

namespace FlowGuardMonitoring.WebHost.Models.Tables;

public class SensorViewModel
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public SensorType Type { get; set; } = SensorType.Unknown;

    [Required]
    public DateTime InstallationDate { get; set; }

    public bool IsActive { get; set; }

    [Required]
    [MaxLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Manufacturer { get; set; }

    [MaxLength(50)]
    public string? ModelNumber { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [Required]
    public string SiteName { get; set; } = string.Empty; // probably dropdown selected value
}