// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AlunoTests
{
    [Theory(DisplayName = "Aluno cria com nomes válidos")]
    [InlineData("  Joao da Silva  ", "Joao da Silva")]
    [InlineData("Maria", "Maria")]
    [InlineData("Ana   Souza", "Ana Souza")]
    [InlineData("Pedro Santos", "Pedro Santos")]
    public void DeveCriarQuandoNomeValido(string nome, string esperado)
    {
        var result = CriarAluno(nome: nome);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value!.Nome);
        Assert.Equal(TestDataFactory.LogradouroValido().Id, result.Value.Endereco.LogradouroId);
    }

    [Theory(DisplayName = "Aluno exige nome")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveFalharQuandoNomeVazio(string? nome)
    {
        var result = CriarAluno(nome: nome);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory(DisplayName = "Aluno exige data de nascimento válida")]
    [InlineData(0, "DATA_NASCIMENTO_OBRIGATORIO")]
    [InlineData(10, "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    [InlineData(11, "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    public void DeveFalharQuandoDataNascimentoInvalida(int idade, string mensagem)
    {
        var data = idade == 0
            ? default
            : DateOnly.FromDateTime(DateTime.Today.AddYears(-idade));
        var result = CriarAluno(dataNascimento: data);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    [Theory(DisplayName = "Aluno aceita idade mínima e superiores")]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(60)]
    public void DeveCriarQuandoIdadeValida(int idade)
    {
        var result = CriarAluno(
            dataNascimento: DateOnly.FromDateTime(DateTime.Today.AddYears(-idade)));

        Assert.True(result.IsSuccess);
    }

    [Theory(DisplayName = "Aluno agrega falhas dos objetos de valor")]
    [InlineData("123", "(11) 91234-5678", "aluno@example.com", "Abcdef", "CPF_DIGITOS")]
    [InlineData("529.982.247-25", "123", "aluno@example.com", "Abcdef", "TELEFONE_DIGITOS")]
    [InlineData("529.982.247-25", "(11) 91234-5678", "email-invalido", "Abcdef", "EMAIL_FORMATO")]
    [InlineData("529.982.247-25", "(11) 91234-5678", "aluno@example.com", "abcdef", "SENHA_FORMATO")]
    public void DeveFalharQuandoObjetoDeValorInvalido(
        string cpf,
        string telefone,
        string email,
        string senha,
        string mensagem)
    {
        var result = CriarAluno(cpf: cpf, telefone: telefone, email: email, senha: senha);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    private static AcademiaDoZe.Domain.Common.Result<Aluno> CriarAluno(
        string? nome = "Joao",
        string? cpf = "529.982.247-25",
        DateOnly? dataNascimento = null,
        string? telefone = "(11) 91234-5678",
        string? email = "aluno@example.com",
        string? senha = "Abcdef") =>
        Aluno.Criar(
            1,
            nome,
            cpf,
            dataNascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
            telefone,
            email,
            TestDataFactory.LogradouroValido(),
            "123",
            string.Empty,
            senha,
            TestDataFactory.ArquivoValido());
}
