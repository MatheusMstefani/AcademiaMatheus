// Matheus Marques Stefani
namespace AcademiaDoZe.Domain.ValueObjects;

public sealed record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        Valor = valor;
    }
}
