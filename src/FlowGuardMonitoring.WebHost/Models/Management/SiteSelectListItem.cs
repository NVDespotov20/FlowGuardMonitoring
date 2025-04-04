using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowGuardMonitoring.WebHost.Models.Management;

public class SiteSelectListItem : SelectListItem
{
    [Required]
    public double Latitude { get; set; }
    [Required]
    public double Longitude { get; set; }
}