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
    public string SiteName { get; set; } = string.Empty;
}