// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory(DisplayName = "Logradouro normaliza campos válidos")]
    [InlineData(" Rua   Teste ", "Rua Teste", " sp ", "SP")]
    [InlineData("Avenida Brasil", "Avenida Brasil", "rj", "RJ")]
    [InlineData("Praça Central", "Praça Central", " m g ", "MG")]
    [InlineData("Rodovia  Um", "Rodovia Um", " pr ", "PR")]
    public void DeveCriarENormalizarCampos(
        string nome,
        string nomeEsperado,
        string estado,
        string estadoEsperado)
    {
        var result = Logradouro.Criar(
            5, "12345-678", nome, " Centro ", " Cidade ", estado, " Brasil ");

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Id);
        Assert.Equal(nomeEsperado, result.Value.Nome);
        Assert.Equal("Centro", result.Value.Bairro);
        Assert.Equal("Cidade", result.Value.Cidade);
        Assert.Equal(estadoEsperado, result.Value.Estado);
        Assert.Equal("Brasil", result.Value.Pais);
    }

    [Theory(DisplayName = "Logradouro exige todos os campos")]
    [InlineData("", "Bairro", "Cidade", "SP", "Brasil", "NOME_OBRIGATORIO")]
    [InlineData("Rua", "", "Cidade", "SP", "Brasil", "BAIRRO_OBRIGATORIO")]
    [InlineData("Rua", "Bairro", "", "SP", "Brasil", "CIDADE_OBRIGATORIO")]
    [InlineData("Rua", "Bairro", "Cidade", "", "Brasil", "ESTADO_OBRIGATORIO")]
    [InlineData("Rua", "Bairro", "Cidade", "SP", "", "PAIS_OBRIGATORIO")]
    public void DeveFalharQuandoCampoObrigatorioVazio(
        string nome,
        string bairro,
        string cidade,
        string estado,
        string pais,
        string mensagem)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, bairro, cidade, estado, pais);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    [Theory(DisplayName = "Logradouro valida o CEP")]
    [InlineData(null, "CEP_OBRIGATORIO")]
    [InlineData("", "CEP_OBRIGATORIO")]
    [InlineData("123", "CEP_DIGITOS")]
    public void DeveFalharQuandoCepInvalido(string? cep, string mensagem)
    {
        var result = Logradouro.Criar(1, cep, "Rua", "Bairro", "Cidade", "SP", "Brasil");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    [Fact(DisplayName = "Entidade rejeita identificador negativo")]
    public void DeveLancarExcecaoQuandoIdNegativo()
    {
        Assert.Throws<DomainException>(() =>
            Logradouro.Criar(-1, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil"));
    }
}
