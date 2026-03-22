using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace LivestreamApp.Shared.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");
        if (!EmailRegex.IsMatch(value))
            throw new DomainException("Email format is invalid.");
        return new Email(value.ToLowerInvariant().Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
