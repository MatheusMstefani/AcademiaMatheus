// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDto(this Matricula entity, AlunoDto aluno)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(aluno);
        return new MatriculaDto
        {
            Id = entity.Id, AlunoMatricula = aluno, Plano = entity.Plano.ToApplication(),
            DataInicio = entity.DataInicio, DataFim = entity.DataFim, Objetivo = entity.Objetivo,
            RestricoesMedicas = entity.RestricoesMedicas.ToApplication(),
            ObservacoesRestricoes = entity.ObservacoesRestricoes, LaudoMedico = entity.LaudoMedico.ToDto()
        };
    }

    public static Matricula ToEntity(this MatriculaDto dto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(aluno);
        return Matricula.Criar(dto.Id, aluno, dto.Plano.ToDomain(), dto.DataInicio,
            dto.Objetivo, dto.RestricoesMedicas.ToDomain(), dto.LaudoMedico.ToArquivo(),
            dto.ObservacoesRestricoes).ValueOrThrow();
    }

    public static Matricula UpdateFromDto(this Matricula entity, MatriculaDto dto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(aluno);
        // Zero é válido: permite voltar ao plano Mensal e retirar todas as restrições.
        // Laudo nulo mantém o anterior. Um arquivo informado precisa ser válido.
        return Matricula.Criar(entity.Id, aluno, dto.Plano.ToDomain(), dto.DataInicio,
            dto.Objetivo, dto.RestricoesMedicas.ToDomain(),
            dto.LaudoMedico is null ? entity.LaudoMedico : dto.LaudoMedico.ToArquivo(),
            dto.ObservacoesRestricoes).ValueOrThrow();
    }
}
