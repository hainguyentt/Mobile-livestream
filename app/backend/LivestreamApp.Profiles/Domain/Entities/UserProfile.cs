using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Events;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Profiles.Domain.Entities;

/// <summary>Represents a user's public-facing profile information.</summary>
public sealed class UserProfile : Entity<Guid>
{
    public string DisplayName { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public string? Bio { get; private set; }
    public string[] Interests { get; private set; } = [];
    public string PreferredLanguage { get; private set; }
    public bool IsProfileComplete { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public ICollection<UserPhoto> Photos { get; private set; } = [];

    private UserProfile(Guid userId, string displayName, DateOnly dateOfBirth) : base(userId)
    {
        DisplayName = displayName;
        DateOfBirth = dateOfBirth;
        PreferredLanguage = "ja";
        IsProfileComplete = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private UserProfile() : base(Guid.Empty) { DisplayName = null!; PreferredLanguage = null!; }

    /// <summary>Creates a new user profile. UserId is used as the entity ID (1-1 with User).</summary>
    /// <exception cref="DomainException">Thrown when displayName is empty or dateOfBirth is invalid.</exception>
    public static UserProfile Create(Guid userId, string displayName, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Display name cannot be empty.");

        if (dateOfBirth >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Date of birth must be in the past.");

        return new UserProfile(userId, displayName.Trim(), dateOfBirth);
    }

    /// <summary>Updates bio and interests. Raises ProfileUpdatedEvent.</summary>
    public void Update(string? bio, string[]? interests, string? preferredLanguage)
    {
        var updatedFields = new List<string>();

        if (bio != Bio) { Bio = bio; updatedFields.Add(nameof(Bio)); }
        if (interests is not null) { Interests = interests; updatedFields.Add(nameof(Interests)); }
        if (preferredLanguage is not null && preferredLanguage != PreferredLanguage)
        {
            PreferredLanguage = preferredLanguage;
            updatedFields.Add(nameof(PreferredLanguage));
        }

        UpdatedAt = DateTime.UtcNow;

        if (updatedFields.Count > 0)
            RaiseDomainEvent(new ProfileUpdatedEvent(Id, [.. updatedFields]));
    }
}
