// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.Validation;

public static class MatriculaCadastroValidation
{
    public static DateOnly CalcularDataFim(AppMatriculaPlano plano, DateOnly inicio)
    {
        if (inicio == default) throw new ArgumentException("Informe a data de início.");
        return plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => throw new ArgumentException("Selecione um plano válido.")
        };
    }

    public static bool ExigeLaudo(MatriculaDto dto) =>
        dto.RestricoesMedicas != AppMatriculaRestricoes.None ||
        (dto.AlunoMatricula is { Id: > 0 } aluno && aluno.DataNascimento != default &&
         aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16)));

    public static IReadOnlyList<string> Validate(MatriculaDto dto)
    {
        var errors = new List<string>();
        if (dto.AlunoMatricula is not { Id: > 0 }) errors.Add("Busque um aluno cadastrado pelo CPF.");
        if (!Enum.IsDefined(dto.Plano)) errors.Add("Selecione um plano válido.");
        if (dto.DataInicio == default) errors.Add("Informe a data de início.");
        else if (Enum.IsDefined(dto.Plano))
        {
            try { CalcularDataFim(dto.Plano, dto.DataInicio); }
            catch (ArgumentOutOfRangeException) { errors.Add("A vigência ultrapassa o limite de datas permitido."); }
        }
        if (string.IsNullOrWhiteSpace(dto.Objetivo)) errors.Add("Informe o objetivo da matrícula.");
        else if (dto.Objetivo.Length > 200) errors.Add("O objetivo deve ter no máximo 200 caracteres.");
        if (dto.ObservacoesRestricoes?.Length > 500) errors.Add("As observações devem ter no máximo 500 caracteres.");
        var valid = Enum.GetValues<AppMatriculaRestricoes>().Aggregate(AppMatriculaRestricoes.None, (a, b) => a | b);
        if ((dto.RestricoesMedicas & ~valid) != 0) errors.Add("Selecione restrições médicas válidas.");
        if (dto.LaudoMedico is not null && dto.LaudoMedico.Conteudo is not { Length: > 0 and <= 15728640 })
            errors.Add("O laudo deve ter conteúdo e no máximo 15 MB.");
        if (ExigeLaudo(dto) && (dto.RemoverLaudoMedico || dto.LaudoMedico?.Conteudo is not { Length: > 0 }))
            errors.Add("Anexe um laudo médico: obrigatório para menores de 16 anos ou alunos com restrições.");
        return errors;
    }
}
