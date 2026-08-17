// Matheus Marques Stefani
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Common;

public class CommonTests
{
    [Theory(DisplayName = "Result Success mantém o valor")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void ResultSuccess_DeveManterValor(int valor)
    {
        var result = Result<int>.Success(valor);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(valor, result.Value);
        Assert.Empty(result.Notifications);
    }

    [Theory(DisplayName = "Result Failure mantém a notificação")]
    [InlineData("Campo", "ERRO")]
    [InlineData("Nome", "NOME_OBRIGATORIO")]
    [InlineData("Cpf", "CPF_INVALIDO")]
    public void ResultFailure_DeveManterNotificacao(string propriedade, string mensagem)
    {
        var result = Result<int>.Failure(propriedade, mensagem);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Notifications, n =>
            n.Propriedade == propriedade && n.Mensagem == mensagem);
    }

    [Fact(DisplayName = "Raízes de agregado implementam a interface marcadora")]
    public void EntidadesPrincipais_DevemSerRaizesDeAgregado()
    {
        Type[] tipos =
        [
            typeof(Logradouro),
            typeof(Aluno),
            typeof(Colaborador),
            typeof(Matricula),
            typeof(AcessoAluno),
            typeof(AcessoColaborador)
        ];

        Assert.All(tipos, tipo => Assert.True(typeof(IAggregateRoot).IsAssignableFrom(tipo)));
    }

    [Fact(DisplayName = "Identificador negativo gera exceção de domínio")]
    public void IdNegativo_DeveGerarDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Logradouro.Criar(-10, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil"));
    }
}
