using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FlowGuardMonitoring.DAL.Models;

public class User : IdentityUser
{
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
    public ICollection<Site> Sites { get; set; } = new List<Site>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}