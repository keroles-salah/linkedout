using System.ComponentModel.DataAnnotations;

namespace Linkdout.Api.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty; // like, comment, connect, share, mention, badge

    [Required, MaxLength(500)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Link { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
