using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;
using System.Text.RegularExpressions;

namespace LivestreamApp.Shared.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    // E.164 format: +[country code][number], e.g. +819012345678
    private static readonly Regex PhoneRegex =
        new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number is required.");
        var normalized = value.Trim();
        if (!PhoneRegex.IsMatch(normalized))
            throw new DomainException("Phone number must be in E.164 format (e.g. +819012345678).");
        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
