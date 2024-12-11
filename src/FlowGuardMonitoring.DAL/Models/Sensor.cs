using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.DAL.Models;

public class Sensor
{
    [Key]
    public int SensorId { get; set; }
    
    [MaxLength(50)]
    public string Name { get; set; } = "";
    [MaxLength(50)]
    public string Type { get; set; } = "";

    public DateTime InstallationDate { get; set; }

    public bool IsActive { get; set; }

    
    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    public int SiteId { get; set; }
    public Site Site { get; set; }
}