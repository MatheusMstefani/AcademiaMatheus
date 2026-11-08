// Matheus Marques Stefani
using System.Globalization;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class RestricoesFlagsDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is AppMatriculaRestricoes flags ? flags.GetDisplayName() : "";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
