namespace DentalClinic.WebApi;

public static class Constants
{
    public const string DefaultConnectionStringName = "DefaultConnection";

    public const string JwtRoleClaimName = "role";
    public static TimeSpan DefaultTokenExpirationTimeout = new(24, 0, 0);

    public const int DefaultPageIndex = 0;
    public const int DefaultPageSize = 20;

}
