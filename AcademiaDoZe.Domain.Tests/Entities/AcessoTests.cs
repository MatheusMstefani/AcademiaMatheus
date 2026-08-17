// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoTests
{
    [Theory(DisplayName = "Acesso de aluno rejeita horários fora do intervalo")]
    [InlineData(0, 0)]
    [InlineData(5, 59)]
    [InlineData(22, 1)]
    [InlineData(23, 59)]
    public void AcessoAluno_DeveFalharQuandoHorarioInvalido(int hora, int minuto)
    {
        var result = AcessoAluno.Criar(
            1,
            TestDataFactory.AlunoValido(),
            DateTime.Today.AddHours(hora).AddMinutes(minuto));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Theory(DisplayName = "Acesso de aluno aceita horários permitidos")]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(22)]
    public void AcessoAluno_DeveCriarQuandoHorarioValido(int hora)
    {
        var aluno = TestDataFactory.AlunoValido(9);
        var dataHora = DateTime.Today.AddHours(hora);
        var result = AcessoAluno.Criar(1, aluno, dataHora);

        Assert.True(result.IsSuccess);
        Assert.Equal(9, result.Value!.AlunoId);
        Assert.Equal(dataHora, result.Value.DataHora);
    }

    [Fact(DisplayName = "Acesso de aluno exige aluno")]
    public void AcessoAluno_DeveFalharQuandoAlunoNulo()
    {
        var result = AcessoAluno.Criar(1, null, DateTime.Today.AddHours(10));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ALUNO_INVALIDO");
    }

    [Theory(DisplayName = "Acesso de colaborador rejeita horários fora do intervalo")]
    [InlineData(0, 0)]
    [InlineData(5, 59)]
    [InlineData(22, 1)]
    [InlineData(23, 59)]
    public void AcessoColaborador_DeveFalharQuandoHorarioInvalido(int hora, int minuto)
    {
        var result = AcessoColaborador.Criar(
            1,
            TestDataFactory.ColaboradorValido(),
            DateTime.Today.AddHours(hora).AddMinutes(minuto));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Theory(DisplayName = "Acesso de colaborador aceita horários permitidos")]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(22)]
    public void AcessoColaborador_DeveCriarQuandoHorarioValido(int hora)
    {
        var colaborador = TestDataFactory.ColaboradorValido(11);
        var dataHora = DateTime.Today.AddHours(hora);
        var result = AcessoColaborador.Criar(1, colaborador, dataHora);

        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value!.ColaboradorId);
        Assert.Equal(dataHora, result.Value.DataHora);
    }

    [Fact(DisplayName = "Acesso de colaborador exige colaborador")]
    public void AcessoColaborador_DeveFalharQuandoColaboradorNulo()
    {
        var result = AcessoColaborador.Criar(1, null, DateTime.Today.AddHours(10));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "COLABORADOR_INVALIDO");
    }
}
