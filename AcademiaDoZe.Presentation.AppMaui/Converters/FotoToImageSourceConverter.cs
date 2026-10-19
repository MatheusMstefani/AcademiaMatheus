// Matheus Marques Stefani
using System.Globalization;
using AcademiaDoZe.Application.DTOs;
namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public sealed class FotoToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var bytes = value is ArquivoDto photo ? photo.Conteudo : value as byte[];
        return bytes is { Length: > 0 } ? ImageSource.FromStream(() => new MemoryStream(bytes, false)) : "person.png";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
