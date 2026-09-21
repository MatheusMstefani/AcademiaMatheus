// Matheus Marques Stefani
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Application.Security;

namespace AcademiaDoZe.Application.Services;

internal static class ServiceValidation
{
    internal static T Require<T>(Result<T> result, string parameter)
    {
        if (result.IsFailure)
            throw new ArgumentException(string.Join("; ",
                result.Notifications.Select(n => $"{n.Propriedade}: {n.Mensagem}")), parameter);
        return result.Value!;
    }

    internal static string HashPassword(string? password)
    {
        Require(Senha.Criar(password), nameof(password));
        // Validar antes do hash; não alterar nem substituir o DTO recebido.
        return PasswordHasher.Hash(password!);
    }
}
