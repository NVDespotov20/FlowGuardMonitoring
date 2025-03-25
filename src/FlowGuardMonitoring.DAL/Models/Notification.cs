using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.DAL.Models;

public class Notification
{
    public int NotificationId { get; set; }
    [MaxLength(100)]
    public required string Title { get; set; }
    [MaxLength(500)]
    public required string Description { get; set; }
    [MaxLength(50)]
    public required string Type { get; set; }
    public required DateTime Date { get; set; }

    public bool IsRead { get; set; }
    public required string UserId { get; set; }
    public User? User { get; set; }
}