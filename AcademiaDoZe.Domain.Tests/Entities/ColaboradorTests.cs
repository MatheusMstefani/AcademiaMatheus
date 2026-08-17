// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    [Theory(DisplayName = "Colaborador aceita combinações válidas")]
    [InlineData(ColaboradorTipo.Administrador, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.Estagio)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.Estagio)]
    public void DeveCriarQuandoTipoEVinculoValidos(
        ColaboradorTipo tipo,
        ColaboradorVinculo vinculo)
    {
        var result = CriarColaborador(tipo: tipo, vinculo: vinculo);

        Assert.True(result.IsSuccess);
        Assert.Equal(tipo, result.Value!.Tipo);
        Assert.Equal(vinculo, result.Value.Vinculo);
    }

    [Theory(DisplayName = "Administrador deve ser CLT")]
    [InlineData(ColaboradorVinculo.Estagio)]
    public void DeveFalharQuandoAdministradorNaoForClt(ColaboradorVinculo vinculo)
    {
        var result = CriarColaborador(
            tipo: ColaboradorTipo.Administrador,
            vinculo: vinculo);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
    }

    [Theory(DisplayName = "Colaborador valida a data de admissão")]
    [InlineData(0, true, "DATA_ADMISSAO_OBRIGATORIO")]
    [InlineData(1, false, "DATA_ADMISSAO_MAIOR_QUE_ATUAL")]
    [InlineData(30, false, "DATA_ADMISSAO_MAIOR_QUE_ATUAL")]
    public void DeveFalharQuandoDataAdmissaoInvalida(
        int dias,
        bool usarDefault,
        string mensagem)
    {
        var data = usarDefault
            ? default
            : DateOnly.FromDateTime(DateTime.Today.AddDays(dias));
        var result = CriarColaborador(dataAdmissao: data);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    [Theory(DisplayName = "Colaborador valida tipo e vínculo")]
    [InlineData(999, (int)ColaboradorVinculo.CLT, "TIPO_COLABORADOR_INVALIDO")]
    [InlineData((int)ColaboradorTipo.Atendente, 999, "VINCULO_COLABORADOR_INVALIDO")]
    [InlineData(-1, -1, "TIPO_COLABORADOR_INVALIDO")]
    public void DeveFalharQuandoEnumInvalido(int tipo, int vinculo, string mensagem)
    {
        var result = CriarColaborador(
            tipo: (ColaboradorTipo)tipo,
            vinculo: (ColaboradorVinculo)vinculo);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    [Theory(DisplayName = "Colaborador normaliza o nome")]
    [InlineData("  Fulano   da Silva ", "Fulano da Silva")]
    [InlineData("Maria", "Maria")]
    [InlineData(" Ana  Souza ", "Ana Souza")]
    public void DeveNormalizarNome(string nome, string esperado)
    {
        var result = CriarColaborador(nome: nome);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.Nome);
    }

    private static AcademiaDoZe.Domain.Common.Result<Colaborador> CriarColaborador(
        string? nome = "Fulano",
        DateOnly? dataAdmissao = null,
        ColaboradorTipo tipo = ColaboradorTipo.Atendente,
        ColaboradorVinculo vinculo = ColaboradorVinculo.CLT) =>
        Colaborador.Criar(
            1,
            nome,
            "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678",
            "colaborador@example.com",
            TestDataFactory.LogradouroValido(),
            "123",
            string.Empty,
            "Abcdef",
            TestDataFactory.ArquivoValido(),
            dataAdmissao ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            tipo,
            vinculo);
}
