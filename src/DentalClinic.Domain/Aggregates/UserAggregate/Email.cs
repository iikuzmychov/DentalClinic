using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DentalClinic.Domain;

public sealed partial record Email
{
    public const int MinLength = 6;
    public const int MaxLength = 320;

    [MaxLength(MaxLength)]
    [MinLength(MinLength)]
    public string Value { get; }

    public Email(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentException("Email length is invalid.", nameof(value));
        }

        if (!Regex().IsMatch(value))
        {
            throw new ArgumentException("Email is invalid.", nameof(value));
        }

        Value = value;
    }

    [GeneratedRegex("^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
    public static partial Regex Regex();
}
