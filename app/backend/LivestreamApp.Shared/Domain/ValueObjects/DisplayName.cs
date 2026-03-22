using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Shared.Domain.ValueObjects;

public sealed class DisplayName : ValueObject
{
    public const int MinLength = 2;
    public const int MaxLength = 30;

    public string Value { get; }

    private DisplayName(string value) => Value = value;

    public static DisplayName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Display name is required.");
        var trimmed = value.Trim();
        if (trimmed.Length < MinLength || trimmed.Length > MaxLength)
            throw new DomainException($"Display name must be between {MinLength} and {MaxLength} characters.");
        return new DisplayName(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
