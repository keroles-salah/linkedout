using System.ComponentModel.DataAnnotations;

namespace Linkdout.Api.Models;

public class Connection
{
    public int Id { get; set; }

    public int RequesterId { get; set; }
    public User Requester { get; set; } = null!;

    public int RecipientId { get; set; }
    public User Recipient { get; set; } = null!;

    // "pending", "accepted", "rejected"
    [Required, MaxLength(20)]
    public string Status { get; set; } = "pending";

    // "close", "learning", "professional"
    [MaxLength(50)]
    public string? Circle { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
}

public class Group
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? CoverColor { get; set; }

    public string? Icon { get; set; }

    [Required, MaxLength(50)]
    public string Privacy { get; set; } = "public"; // public, private

    public int MemberCount { get; set; } = 0;
    public int PostCount { get; set; } = 0;

    public int CreatorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}

public class GroupMember
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "member"; // admin, moderator, member

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Company
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Industry { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Size { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverColor { get; set; }

    public int FollowerCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}

public class XpTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty; // like, comment, post, share, connect, join_group, profile_edit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Job
{
    public int Id { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Type { get; set; } = "full-time"; // full-time, part-time, internship, freelance

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    // JSON array of required skills
    public string? RequiredSkills { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}
