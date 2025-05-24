namespace DentalClinic.Infrastructure.Extensions;

internal static class TypeExtensions
{
    public static IEnumerable<Type> GetInheritedTypes(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var hierarchy = new List<Type>();
        var currentType = type;

        while (currentType.BaseType != null)
        {
            hierarchy.Add(currentType);
            currentType = currentType.BaseType;
        }

        return hierarchy;
    }
}
