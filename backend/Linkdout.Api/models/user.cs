using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linkdout.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Headline { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    [MaxLength(50)]
    public string? CoverColor { get; set; } = "#7C3AED";

    // Opportunity Status: "open", "growing", "available", "learning"
    [MaxLength(50)]
    public string? Status { get; set; } = "open";

    // Admin
    [MaxLength(50)]
    public string Role { get; set; } = "user"; // user, admin

    public bool IsActive { get; set; } = true;

    // Gamification
    public int XP { get; set; } = 0;
    public string? Badges { get; set; } // JSON array of badge names

    // Skills stored as JSON array string
    public string? Skills { get; set; }

    public int ProfileViews { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }

    // Navigation
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Connection> SentConnections { get; set; } = new List<Connection>();
    public ICollection<Connection> ReceivedConnections { get; set; } = new List<Connection>();
    public ICollection<Experience> Experiences { get; set; } = new List<Experience>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
}

public class Experience
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Type { get; set; } // work, internship, volunteer, education

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
