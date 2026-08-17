// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory(DisplayName = "Cep obrigatório")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Cep_DeveFalharQuandoVazio(string? input)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cep deve possuir oito dígitos")]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("12-345")]
    [InlineData("abcdefgh")]
    [InlineData("12a34b")]
    public void Cep_DeveFalharQuandoQuantidadeDigitosInvalida(string input)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_DIGITOS");
    }

    [Theory(DisplayName = "Cep aceita formatos válidos e normaliza")]
    [InlineData("12345-678", "12345678")]
    [InlineData("12345678", "12345678")]
    [InlineData("12.345-678", "12345678")]
    [InlineData(" 12345-678 ", "12345678")]
    [InlineData("87654-321", "87654321")]
    [InlineData("00000-000", "00000000")]
    public void Cep_DeveCriarQuandoValido(string input, string expected)
    {
        var result = Cep.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }

    [Theory(DisplayName = "Cpf obrigatório")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Cpf_DeveFalharQuandoVazio(string? input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory(DisplayName = "Cpf deve possuir onze dígitos")]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("sem digitos")]
    [InlineData("123.456")]
    public void Cpf_DeveFalharQuandoQuantidadeDigitosInvalida(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }

    [Theory(DisplayName = "Cpf rejeita dígitos verificadores inválidos")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    public void Cpf_DeveFalharQuandoInvalido(string input)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Theory(DisplayName = "Cpf aceita documentos válidos e normaliza")]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData("111.444.777-35", "11144477735")]
    [InlineData("935.411.347-80", "93541134780")]
    [InlineData("168.995.350-09", "16899535009")]
    public void Cpf_DeveCriarQuandoValido(string input, string expected)
    {
        var result = Cpf.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }

    [Theory(DisplayName = "Telefone obrigatório")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Telefone_DeveFalharQuandoVazio(string? input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }

    [Theory(DisplayName = "Telefone deve possuir onze dígitos")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    [InlineData("1191234567")]
    [InlineData("119123456789")]
    [InlineData("telefone")]
    [InlineData("11 9999-9999")]
    public void Telefone_DeveFalharQuandoQuantidadeDigitosInvalida(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_DIGITOS");
    }

    [Theory(DisplayName = "Telefone aceita formatos válidos")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    [InlineData("11 91234 5678")]
    [InlineData("(21) 99876-5432")]
    [InlineData("21998765432")]
    [InlineData(" 11-91234-5678 ")]
    public void Telefone_DeveCriarQuandoValido(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value!.Valor.Length);
    }

    [Theory(DisplayName = "Email rejeita formatos inválidos")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("semarroba")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@dominio")]
    [InlineData("usuario@.com")]
    [InlineData("usuario@dominio.")]
    [InlineData("a@@b.com")]
    [InlineData("a@b..com")]
    public void Email_DeveFalharQuandoFormatoInvalido(string? input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory(DisplayName = "Email aceita formatos válidos e normaliza")]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("USER@EXAMPLE.COM", "user@example.com")]
    [InlineData(" user@example.com ", "user@example.com")]
    [InlineData("nome.sobrenome@dominio.com.br", "nome.sobrenome@dominio.com.br")]
    [InlineData("a@b.com", "a@b.com")]
    [InlineData(" aluno @ example.com ", "aluno@example.com")]
    public void Email_DeveCriarQuandoValido(string input, string expected)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }

    [Theory(DisplayName = "Senha rejeita valores obrigatórios ou fracos")]
    [InlineData(null, "SENHA_OBRIGATORIO")]
    [InlineData("", "SENHA_OBRIGATORIO")]
    [InlineData(" ", "SENHA_OBRIGATORIO")]
    [InlineData("abcde", "SENHA_FORMATO")]
    [InlineData("abcdef", "SENHA_FORMATO")]
    [InlineData("123456", "SENHA_FORMATO")]
    [InlineData("Abcd", "SENHA_FORMATO")]
    public void Senha_DeveFalharQuandoInvalida(string? input, string mensagem)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == mensagem);
    }

    [Theory(DisplayName = "Senha aceita formatos válidos")]
    [InlineData("Abcdef", "Abcdef")]
    [InlineData("ABCDEF", "ABCDEF")]
    [InlineData("Senha123", "Senha123")]
    [InlineData(" Teste1 ", "Teste1")]
    [InlineData("Minha Senha", "Minha Senha")]
    [InlineData("A12345", "A12345")]
    public void Senha_DeveCriarQuandoValida(string input, string expected)
    {
        var result = Senha.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }

    [Fact(DisplayName = "Arquivo obrigatório quando nulo")]
    public void Arquivo_DeveFalharQuandoNulo()
    {
        var result = Arquivo.Criar(null);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Fact(DisplayName = "Arquivo obrigatório quando vazio")]
    public void Arquivo_DeveFalharQuandoVazio()
    {
        var result = Arquivo.Criar([]);

        Assert.True(result.IsFailure);
    }

    [Fact(DisplayName = "Arquivo aceita conteúdo válido")]
    public void Arquivo_DeveCriarQuandoValido()
    {
        var result = Arquivo.Criar([1, 2, 3]);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Conteudo.Length);
    }

    [Fact(DisplayName = "Arquivo rejeita tamanho superior a quinze megabytes")]
    public void Arquivo_DeveFalharQuandoMuitoGrande()
    {
        var result = Arquivo.Criar(new byte[(15 * 1024 * 1024) + 1]);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }

    [Theory(DisplayName = "Endereco exige logradouro")]
    [InlineData("1")]
    [InlineData("10")]
    public void Endereco_DeveFalharSemLogradouro(string numero)
    {
        var result = Endereco.Criar(null, numero, "");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "LOGRADOURO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Endereco exige número")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Endereco_DeveFalharSemNumero(string? numero)
    {
        var result = Endereco.Criar(TestDataFactory.LogradouroValido(), numero, "");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NUMERO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Endereco cria e normaliza dados válidos")]
    [InlineData("10", "Bloco A", "10", "Bloco A")]
    [InlineData("  10 A  ", "  Bloco   B  ", "10 A", "Bloco B")]
    [InlineData("1", "", "1", "")]
    public void Endereco_DeveCriarQuandoValido(
        string numero,
        string complemento,
        string numeroEsperado,
        string complementoEsperado)
    {
        var logradouro = TestDataFactory.LogradouroValido(7);
        var result = Endereco.Criar(logradouro, numero, complemento);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.LogradouroId);
        Assert.Equal(numeroEsperado, result.Value.Numero);
        Assert.Equal(complementoEsperado, result.Value.Complemento);
    }
}
