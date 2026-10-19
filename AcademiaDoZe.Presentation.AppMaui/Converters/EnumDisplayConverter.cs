// Matheus Marques Stefani
using System.Globalization;
using AcademiaDoZe.Application.Enums;
namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class EnumDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Enum item ? item.GetDisplayName() : "";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
