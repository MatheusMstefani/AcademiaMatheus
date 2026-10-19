// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
namespace AcademiaDoZe.Presentation.AppMaui.Services;

public static class PhotoService
{
    public static async Task<ArquivoDto?> SelectAsync(bool camera)
    {
        FileResult? file;
        if (camera)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                throw new InvalidOperationException("A câmera não está disponível neste dispositivo. Use a opção Galeria.");
            file = await MediaPicker.Default.CapturePhotoAsync();
        }
        else file = await FilePicker.Default.PickAsync(new PickOptions
        { PickerTitle = "Selecione sua foto", FileTypes = FilePickerFileType.Images });
        if (file is null) return null;
        // Limite também durante a leitura: arquivos grandes não ocupam memória sem limite.
        using var input = await file.OpenReadAsync();
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await input.ReadAsync(buffer)) > 0)
        {
            if (output.Length + read > 15 * 1024 * 1024)
                throw new ArgumentException("A foto deve ter no máximo 15 MB.");
            await output.WriteAsync(buffer.AsMemory(0, read));
        }
        var bytes = output.ToArray();
        var image = (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) ||
            (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) ||
            (bytes.Length >= 6 && System.Text.Encoding.ASCII.GetString(bytes, 0, 6) is "GIF87a" or "GIF89a") ||
            (bytes.Length >= 2 && bytes[0] == 'B' && bytes[1] == 'M');
        if (!image) throw new ArgumentException("Selecione uma imagem JPEG, PNG, GIF ou BMP válida.");
        return new ArquivoDto { Conteudo = bytes };
    }
}
