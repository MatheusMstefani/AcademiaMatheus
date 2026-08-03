// Matheus Marques Stefani
namespace AcademiaDoZe.Domain.ValueObjects;

public sealed record Telefone
{
    public string Valor { get; }

    public Telefone(string valor)
    {
        Valor = valor;
    }
}
