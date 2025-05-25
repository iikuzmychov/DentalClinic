namespace DentalClinic.WebApi;

public static class Constants
{
    public const string DefaultConnectionStringName = "DefaultConnection";

    public const string JwtRoleClaimName = "role";
    public static readonly TimeSpan DefaultTokenExpirationTimeout = TimeSpan.FromDays(5);

    public const int DefaultPageIndex = 0;
    public const int DefaultPageSize = 20;
}
