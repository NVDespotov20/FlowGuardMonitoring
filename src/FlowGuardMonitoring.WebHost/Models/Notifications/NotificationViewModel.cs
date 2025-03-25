using System.ComponentModel.DataAnnotations;

namespace FlowGuardMonitoring.WebHost.Models.Notifications;

public class NotificationViewModel
{
    [Key]
    public required int NotificationId { get; set; }
    [MaxLength(50)]
    public required string Type { get; set; }
    [MaxLength(500)]
    public required string Message { get; set; }
    public required DateTime Time { get; set; }

    public bool IsRead { get; set; }
}