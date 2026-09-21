// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new LogradouroDto
        {
            Id = entity.Id, Cep = entity.Cep.Valor, Nome = entity.Nome,
            Bairro = entity.Bairro, Cidade = entity.Cidade, Estado = entity.Estado, Pais = entity.Pais
        };
    }

    public static Logradouro ToEntity(this LogradouroDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Logradouro.Criar(dto.Id, dto.Cep, dto.Nome, dto.Bairro,
            dto.Cidade, dto.Estado, dto.Pais).ValueOrThrow();
    }

    public static Logradouro UpdateFromDto(this Logradouro entity, LogradouroDto dto)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);
        return Logradouro.Criar(entity.Id, dto.Cep, dto.Nome, dto.Bairro,
            dto.Cidade, dto.Estado, dto.Pais).ValueOrThrow();
    }
}
