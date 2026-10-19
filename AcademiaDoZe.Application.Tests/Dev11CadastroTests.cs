// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Validation;
using AcademiaDoZe.Application.Security;
namespace AcademiaDoZe.Application.Tests;

public sealed class Dev11CadastroTests : ApplicationTestBase
{
    // Imagem mínima de teste, sem foto ou informação pessoal real.
    private static byte[] Photo => Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jQfQAAAAASUVORK5CYII=");

    [Fact]
    public void FormularioVazioMostraVariosErros()
    {
        var person = new AlunoDto { Nome="", Cpf="", Telefone="", Numero="", DataNascimento=default };
        var errors = PessoaCadastroValidation.Validate(person, "");
        Assert.Contains(errors, x => x.Contains("nome"));
        Assert.Contains(errors, x => x.Contains("CPF"));
        Assert.Contains(errors, x => x.Contains("foto"));
        Assert.Contains(errors, x => x.Contains("CEP"));
        Assert.True(errors.Count >= 8);
    }

    [Fact]
    public void EdicaoSemSenhaPermiteManterSenhaAtual()
    {
        var dto = NovoAluno(NovoLogradouro());
        dto.Id = 1; dto.Endereco!.Id = 1; dto.Senha = null; dto.Foto = new ArquivoDto { Conteudo = Photo };
        Assert.Empty(PessoaCadastroValidation.Validate(dto, ""));
        Assert.Contains(PessoaCadastroValidation.Validate(dto, "esquecida"), x => x.Contains("confirmação"));
    }

    [Fact]
    public void AdministradorEstagiarioEAdmissaoFuturaSaoRejeitadosJuntos()
    {
        var dto = NovoColaborador(NovoLogradouro());
        dto.Endereco!.Id = 1; dto.Tipo = AppColaboradorTipo.Administrador;
        dto.Vinculo = AppColaboradorVinculo.Estagio; dto.DataAdmissao = Hoje.AddDays(1);
        var errors = PessoaCadastroValidation.Validate(dto, dto.Senha!);
        Assert.Contains(errors, x => x.Contains("CLT"));
        Assert.Contains(errors, x => x.Contains("admissão"));
    }

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("88010-009", "88010009")]
    public void CamposFormatadosMantemApenasDigitos(string input, string expected) =>
        Assert.Equal(expected, PessoaCadastroValidation.Digits(input));

    [Fact]
    public async Task AlunoCompletaRoteiroComFotoBuscaEdicaoEExclusao()
    {
        var address = await Logradouros.AdicionarAsync(NovoLogradouro("88010011"));
        var input = NovoAluno(address); input.Foto = new ArquivoDto { Conteudo = Photo };
        var saved = await Alunos.AdicionarAsync(input);
        var found = await Alunos.ObterPorCpfAsync("529.982.247-25");
        Assert.Equal(saved.Id, found!.Id); Assert.Equal(Photo, found.Foto!.Conteudo);
        Assert.Null(found.Senha);
        found.Nome = "zé dos testes";
        var otherPhoto = Photo.Concat(new byte[] { 0 }).ToArray();
        found.Foto = new ArquivoDto { Conteudo = otherPhoto };
        await Alunos.AtualizarAsync(found);
        var edited = await Alunos.ObterPorIdAsync(saved.Id);
        Assert.Equal("zé dos testes", edited!.Nome); Assert.Equal(otherPhoto, edited.Foto!.Conteudo);
        Assert.Single(await Alunos.ObterPorNomeAsync("testes"));
        Assert.True(await Alunos.RemoverAsync(saved.Id));
        Assert.Null(await Alunos.ObterPorCpfAsync(input.Cpf));
    }

    [Fact]
    public async Task ColaboradorCompletaRoteiroEPreservaSenhaNaEdicao()
    {
        var address = await Logradouros.AdicionarAsync(NovoLogradouro("88010012"));
        var input = NovoColaborador(address); input.Foto = new ArquivoDto { Conteudo = Photo };
        var saved = await Colaboradores.AdicionarAsync(input);
        var found = await Colaboradores.ObterPorCpfAsync("529.982.247-25");
        Assert.Equal(Photo, found!.Foto!.Conteudo); Assert.Null(found.Senha);
        found.Nome = "zé dos testes"; found.Tipo = AppColaboradorTipo.Instrutor;
        found.Vinculo = AppColaboradorVinculo.Estagio; found.Senha = null;
        var otherPhoto = Photo.Concat(new byte[] { 0 }).ToArray();
        found.Foto = new ArquivoDto { Conteudo = otherPhoto };
        await Colaboradores.AtualizarAsync(found);
        var edited = await Colaboradores.ObterPorIdAsync(saved.Id);
        Assert.Equal("zé dos testes", edited!.Nome); Assert.Equal(otherPhoto, edited.Foto!.Conteudo);
        Assert.Equal(AppColaboradorVinculo.Estagio, edited.Vinculo);
        Assert.Single(await Colaboradores.ObterPorTipoAsync(AppColaboradorTipo.Instrutor));
        Assert.Single(await Colaboradores.ObterPorVinculoAsync(AppColaboradorVinculo.Estagio));
        Assert.True(await Colaboradores.RemoverAsync(saved.Id));
        Assert.Null(await Colaboradores.ObterPorCpfAsync(input.Cpf));
    }
}
