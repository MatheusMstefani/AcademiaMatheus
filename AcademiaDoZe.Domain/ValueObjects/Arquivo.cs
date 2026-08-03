// Matheus Marques Stefani
namespace AcademiaDoZe.Domain.ValueObjects;

public sealed record Arquivo
{
    public string Nome { get; }
    public string TipoConteudo { get; }
    public byte[] Conteudo { get; }

    public Arquivo(string nome, string tipoConteudo, byte[] conteudo)
    {
        Nome = nome;
        TipoConteudo = tipoConteudo;
        Conteudo = conteudo;
    }
}
