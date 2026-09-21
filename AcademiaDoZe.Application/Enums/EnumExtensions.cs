// Matheus Marques Stefani
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AcademiaDoZe.Application.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var type = value.GetType();
        var field = type.GetField(value.ToString());
        var name = field?.GetCustomAttribute<DisplayAttribute>()?.GetName();
        if (name is not null) return name;
        if (type.IsDefined(typeof(FlagsAttribute), false))
        {
            var names = Enum.GetValues(type).Cast<Enum>()
                .Where(flag => Convert.ToInt64(flag) != 0 && value.HasFlag(flag))
                .Select(flag => flag.GetDisplayName()).ToArray();
            if (names.Length > 0) return string.Join(", ", names);
        }
        return value.ToString();
    }
}
