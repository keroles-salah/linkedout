namespace Linkdout.Api.DTOs;

// ── Auth ──
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

// ── User ──
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Status { get; set; }
    public List<string> Skills { get; set; } = new();
    public int ProfileViews { get; set; }
    public int ConnectionCount { get; set; }
    public int XP { get; set; }
    public int Level { get; set; }
    public List<string> Badges { get; set; } = new();
    public List<ExperienceDto> Experiences { get; set; } = new();
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public string? Status { get; set; }
    public string? Skills { get; set; }
}

public class ExperienceDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

public class AddExperienceRequest
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
}

// ── Post ──
public class CreatePostRequest
{
    public string Body { get; set; } = string.Empty;
    public List<string>? Images { get; set; }
    public List<string>? Tags { get; set; }
    public int? GroupId { get; set; }
    public PollData? Poll { get; set; }
}

public class PollData
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

public class UpdatePostRequest
{
    public string? Body { get; set; }
    public List<string>? Tags { get; set; }
}

public class PostDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorAvatar { get; set; }
    public string? AuthorHeadline { get; set; }
    public UserBriefDto Author { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public List<string>? Images { get; set; }
    public List<string>? Tags { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsLiked { get; set; }
    public bool IsEdited { get; set; }
    public int? GroupId { get; set; }
    public object? Poll { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserBriefDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? AvatarUrl { get; set; }
}

public class AddCommentRequest
{
    public string Body { get; set; } = string.Empty;
}

public class CommentDto
{
    public int Id { get; set; }
    public UserBriefDto User { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ── Connection ──
public class ConnectionDto
{
    public int Id { get; set; }
    public UserBriefDto User { get; set; } = null!;
    public string Status { get; set; } = string.Empty;
    public string? Circle { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Group ──
public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverColor { get; set; }
    public string? Icon { get; set; }
    public string Privacy { get; set; } = "public";
}

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverColor { get; set; }
    public string? Icon { get; set; }
    public string Privacy { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int PostCount { get; set; }
    public bool IsMember { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Company ──
public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Size { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverColor { get; set; }
    public int FollowerCount { get; set; }
    public int JobCount { get; set; }
}

public class JobDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public List<string>? RequiredSkills { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Search ──
public class SearchResponse
{
    public List<UserDto> People { get; set; } = new();
    public List<JobDto> Jobs { get; set; } = new();
    public List<GroupDto> Groups { get; set; } = new();
    public List<CompanyDto> Companies { get; set; } = new();
}

// ── Pagination ──
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => Page * PageSize < TotalCount;
}
