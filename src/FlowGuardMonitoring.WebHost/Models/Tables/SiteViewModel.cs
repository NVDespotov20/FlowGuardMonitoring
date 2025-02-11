using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.WebHost.Models.Tables;

public class SiteViewModel
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }
}