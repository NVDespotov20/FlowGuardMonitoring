using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.DAL.Models;

public class Site
{
    [Key]
    public int SiteId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(150)]
    public string Description { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();

    public required string UserId { get; set; } = null!;
    public required User User { get; set; }
}