using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Models;

public class Sensor
{
    [Key]
    public int SensorId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public SensorType Type { get; set; } = SensorType.Unknown;

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

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    public int SiteId { get; set; }
    public required Site Site { get; set; }
}