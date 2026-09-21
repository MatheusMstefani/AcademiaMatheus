// Matheus Marques Stefani
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.DTOs;

public abstract class PessoaDto
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cpf { get; set; }
    public required DateOnly DataNascimento { get; set; }
    public required string Telefone { get; set; }
    public string? Email { get; set; }
    public LogradouroDto? Endereco { get; set; }
    public required string Numero { get; set; }
    public string? Complemento { get; set; }
    // Apenas entrada: consultas nunca devolvem senha nem hash.
    public string? Senha { get; set; }
    public ArquivoDto? Foto { get; set; }
}
