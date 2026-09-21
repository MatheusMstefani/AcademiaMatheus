// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

internal static class MappingValidation
{
    internal static T ValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(string.Join("; ",
                result.Notifications.Select(n => $"{n.Propriedade}: {n.Mensagem}")));
        return result.Value!;
    }

    internal static Arquivo? ToArquivo(this ArquivoDto? dto) =>
        dto is null ? null : Arquivo.Criar(dto.Conteudo).ValueOrThrow();

    internal static ArquivoDto? ToDto(this Arquivo? arquivo) =>
        arquivo is null ? null : new ArquivoDto { Conteudo = arquivo.Conteudo.ToArray() };
}
