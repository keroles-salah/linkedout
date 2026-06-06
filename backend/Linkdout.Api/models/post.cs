using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linkdout.Api.Models;

public class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(5000)]
    public string Body { get; set; } = string.Empty;

    // JSON array of image URLs
    public string? Images { get; set; }

    // JSON array of tag strings
    public string? Tags { get; set; }

    public int? GroupId { get; set; }
    public Group? Group { get; set; }

    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    public int SaveCount { get; set; } = 0;

    // JSON poll: {"question":"...","options":["a","b"],"votes":[0,3]}
    public string? Poll { get; set; }
    public bool IsEdited { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}

public class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Like
{
    public int Id { get; set; }

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string? ReactionType { get; set; } = "like"; // like, love, insightful, awesome

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PostBookmark
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
