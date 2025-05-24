using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DentalClinic.Domain;

public sealed partial record SecurePassword
{
    private const int MinLength = 8;
    private const int MaxLength = 64;

    [MinLength(MinLength)]
    [MaxLength(MaxLength)]
    public string Value { get; private set; }

    public SecurePassword(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentException("Password length is invalid.", nameof(value));
        }

        if (!Regex().IsMatch(value))
        {
            throw new ArgumentException("Password is invalid.", nameof(value));
        }

        Value = value;
    }

    [GeneratedRegex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-])[A-Za-z0-9#?!@$%^&*-]+$")]
    private static partial Regex Regex();
}
