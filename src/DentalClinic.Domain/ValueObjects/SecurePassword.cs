using DentalClinic.Domain.JsonConverters;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DentalClinic.Domain;

[JsonConverter(typeof(JsonSecurePasswordConverter))]
public sealed partial record SecurePassword : IParsable<SecurePassword>
{
    public const int MinLength = 8;
    public const int MaxLength = 64;

    [MinLength(MinLength)]
    [MaxLength(MaxLength)]
    public string Value { get; private set; }

    private SecurePassword(string value)
    {
        Value = value;
    }

    public static SecurePassword Parse(string value) => Parse(value, null);

    public static SecurePassword Parse(string value, IFormatProvider? provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentException("Password length is invalid.", nameof(value));
        }

        if (!Regex().IsMatch(value))
        {
            throw new FormatException("Password format is invalid.");
        }

        return new SecurePassword(value);
    }

    public static bool TryParse([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out SecurePassword result)
        => TryParse(value, null, out result);

    public static bool TryParse(
        [NotNullWhen(true)] string? value,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out SecurePassword result)
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

    [GeneratedRegex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-])[A-Za-z0-9#?!@$%^&*-]+$")]
    public static partial Regex Regex();
}
