using DentalClinic.Domain.JsonConverters;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DentalClinic.Domain;

[JsonConverter(typeof(JsonEmailConverter))]
public sealed partial record Email : IParsable<Email>
{
    public const int MinLength = 6;
    public const int MaxLength = 320;

    [MaxLength(MaxLength)]
    [MinLength(MinLength)]
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Parse(string value) => Parse(value, null);

    public static Email Parse(string value, IFormatProvider? provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentException("Email length is invalid.", nameof(value));
        }

        if (!Regex().IsMatch(value))
        {
            throw new FormatException("Email format is invalid.");
        }

        return new Email(value);
    }

    public static bool TryParse([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out Email result)
        => TryParse(value, null, out result);

    public static bool TryParse(
        [NotNullWhen(true)] string? value,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out Email result)
    {
        try
        {
            result = Parse(value!, provider);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
    public static partial Regex Regex();
}
