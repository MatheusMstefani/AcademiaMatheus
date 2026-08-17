// Matheus Marques Stefani
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.Tests.Services;

public class NormalizacaoServiceTests
{
    [Theory(DisplayName = "TextoVazioOuNulo identifica textos vazios e preenchidos")]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("   ", true)]
    [InlineData("texto", false)]
    [InlineData("  texto  ", false)]
    public void TextoVazioOuNulo_DeveRetornarEsperado(string? input, bool expected)
    {
        Assert.Equal(expected, NormalizacaoService.TextoVazioOuNulo(input));
    }

    [Theory(DisplayName = "LimparEspacos remove espaços extras")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  a  b  c  ", "a b c")]
    [InlineData("a\tb\nc", "a b c")]
    [InlineData(" texto simples ", "texto simples")]
    public void LimparEspacos_DeveNormalizar(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.LimparEspacos(input));
    }

    [Theory(DisplayName = "LimparTodosEspacos remove todos os espaços")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("a b c", "abc")]
    [InlineData("  a  b  ", "ab")]
    [InlineData("a\tb\nc", "abc")]
    [InlineData(" sem espacos ", "semespacos")]
    public void LimparTodosEspacos_DeveNormalizar(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.LimparTodosEspacos(input));
    }

    [Theory(DisplayName = "ParaMaiusculo converte o texto")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("abc", "ABC")]
    [InlineData("Sp", "SP")]
    [InlineData("áéíõç", "ÁÉÍÕÇ")]
    public void ParaMaiusculo_DeveConverter(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.ParaMaiusculo(input));
    }

    [Theory(DisplayName = "ParaMinusculo converte o texto")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("ABC", "abc")]
    [InlineData("EMAIL@TESTE.COM", "email@teste.com")]
    [InlineData("ÁÉÍÕÇ", "áéíõç")]
    public void ParaMinusculo_DeveConverter(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.ParaMinusculo(input));
    }

    [Theory(DisplayName = "LimparEDigitos mantém somente números")]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("a1b2c3", "123")]
    [InlineData("(11) 91234-5678", "11912345678")]
    [InlineData("123.456.789-00", "12345678900")]
    [InlineData("no-digits", "")]
    [InlineData(" 0 1 2 ", "012")]
    public void LimparEDigitos_DeveManterApenasDigitos(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.LimparEDigitos(input));
    }
}
