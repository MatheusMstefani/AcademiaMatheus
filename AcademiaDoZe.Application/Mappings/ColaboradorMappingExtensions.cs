// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDto(this Colaborador entity, Logradouro? logradouro = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new ColaboradorDto
        {
            Id = entity.Id, Nome = entity.Nome, Cpf = entity.Cpf.Valor,
            DataNascimento = entity.DataNascimento, Telefone = entity.Telefone.Valor,
            Email = entity.Email.Valor, Endereco = logradouro?.ToDto(),
            Numero = entity.Endereco.Numero, Complemento = entity.Endereco.Complemento,
            Senha = null, Foto = entity.Foto.ToDto(),
            DataAdmissao = entity.DataAdmissao, Tipo = entity.Tipo.ToApplication(),
            Vinculo = entity.Vinculo.ToApplication()
        };
    }

    public static Colaborador ToEntity(this ColaboradorDto dto, Logradouro? logradouro = null, string? senhaHash = null)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var endereco = logradouro ?? dto.Endereco?.ToEntity()
            ?? throw new InvalidOperationException("Logradouro é obrigatório.");
        return Colaborador.Criar(dto.Id, dto.Nome, dto.Cpf, dto.DataNascimento,
            dto.Telefone, dto.Email, endereco, dto.Numero, dto.Complemento,
            senhaHash ?? dto.Senha, dto.Foto.ToArquivo()!,
            dto.DataAdmissao, dto.Tipo.ToDomain(), dto.Vinculo.ToDomain()).ValueOrThrow();
    }

    public static Colaborador UpdateFromDto(this Colaborador entity, ColaboradorDto dto,
        Logradouro? logradouro = null, string? senhaHash = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);
        var endereco = logradouro ?? dto.Endereco?.ToEntity()
            ?? throw new InvalidOperationException("Logradouro é obrigatório.");
        // CPF identifica a pessoa e não é alterado na edição.
        return Colaborador.Criar(entity.Id, dto.Nome, entity.Cpf.Valor, dto.DataNascimento,
            dto.Telefone, dto.Email ?? entity.Email.Valor, endereco, dto.Numero, dto.Complemento,
            senhaHash ?? (string.IsNullOrWhiteSpace(dto.Senha) ? entity.Senha.Valor : dto.Senha),
            dto.Foto is null ? entity.Foto : dto.Foto.ToArquivo()!,
            dto.DataAdmissao, dto.Tipo.ToDomain(), dto.Vinculo.ToDomain()).ValueOrThrow();
    }
}
