// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    [Theory(DisplayName = "Matrícula calcula o fim pelo plano")]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public void DeveCalcularDataFim(MatriculaPlano plano, int meses)
    {
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var aluno = TestDataFactory.AlunoValido(8);
        var result = Matricula.Criar(
            1, aluno, plano, inicio, "Objetivo", MatriculaRestricoes.None, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
        Assert.Equal(8, result.Value.AlunoId);
    }

    [Theory(DisplayName = "Matrícula rejeita plano inválido")]
    [InlineData(99)]
    [InlineData(-1)]
    public void DeveFalharQuandoPlanoInvalido(int plano)
    {
        var result = Matricula.Criar(
            1,
            TestDataFactory.AlunoValido(),
            (MatriculaPlano)plano,
            DateOnly.FromDateTime(DateTime.Today),
            "Objetivo",
            MatriculaRestricoes.None,
            null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "PLANO_INVALIDO");
    }

    [Fact(DisplayName = "Matrícula exige data de início")]
    public void DeveFalharSemDataInicio()
    {
        var result = Matricula.Criar(
            1,
            TestDataFactory.AlunoValido(),
            MatriculaPlano.Mensal,
            default,
            "Objetivo",
            MatriculaRestricoes.None,
            null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_INICIO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Matrícula exige objetivo")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveFalharSemObjetivo(string? objetivo)
    {
        var result = Matricula.Criar(
            1,
            TestDataFactory.AlunoValido(),
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            objetivo,
            MatriculaRestricoes.None,
            null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "OBJETIVO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Menor de dezesseis anos exige laudo")]
    [InlineData(12, true)]
    [InlineData(15, true)]
    [InlineData(16, false)]
    [InlineData(20, false)]
    public void DeveValidarLaudoPelaIdade(int idade, bool deveFalhar)
    {
        var result = Matricula.Criar(
            1,
            TestDataFactory.AlunoValido(idade: idade),
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Objetivo",
            MatriculaRestricoes.None,
            null);

        Assert.Equal(deveFalhar, result.IsFailure);
        if (deveFalhar)
            Assert.Contains(result.Notifications, n => n.Mensagem == "MENOR_16_LAUDO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Restrições exigem laudo")]
    [InlineData(MatriculaRestricoes.Diabetes)]
    [InlineData(MatriculaRestricoes.Alergias)]
    [InlineData(MatriculaRestricoes.PressaoAlta | MatriculaRestricoes.RemedioContinuo)]
    public void DeveFalharQuandoRestricaoSemLaudo(MatriculaRestricoes restricoes)
    {
        var result = CriarComRestricao(restricoes, null, "observação");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "RESTRICOES_LAUDO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Restrições com laudo são aceitas")]
    [InlineData(MatriculaRestricoes.Diabetes)]
    [InlineData(MatriculaRestricoes.Alergias)]
    [InlineData(MatriculaRestricoes.Diabetes | MatriculaRestricoes.Alergias)]
    public void DeveCriarQuandoRestricaoPossuiLaudo(MatriculaRestricoes restricoes)
    {
        var result = CriarComRestricao(
            restricoes,
            TestDataFactory.ArquivoValido(),
            "  observacao   medica  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("observacao medica", result.Value!.ObservacoesRestricoes);
    }

    [Theory(DisplayName = "Matrícula rejeita restrições desconhecidas")]
    [InlineData(64)]
    [InlineData(128)]
    public void DeveFalharQuandoRestricaoInvalida(int restricao)
    {
        var result = CriarComRestricao(
            (MatriculaRestricoes)restricao,
            TestDataFactory.ArquivoValido(),
            "observação");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "RESTRICOES_INVALIDAS");
    }

    private static AcademiaDoZe.Domain.Common.Result<Matricula> CriarComRestricao(
        MatriculaRestricoes restricoes,
        Arquivo? laudo,
        string observacoes) =>
        Matricula.Criar(
            1,
            TestDataFactory.AlunoValido(),
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            "Objetivo",
            restricoes,
            laudo,
            observacoes);
}
