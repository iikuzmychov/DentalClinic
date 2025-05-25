using System.Reflection;

namespace DentalClinic.WebApi;

internal static class EnvironmentHelper
{
    // https://github.com/dotnet/aspnetcore/issues/54698
    public static bool IsOpenApiGeneration()
        => Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
}
