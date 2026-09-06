// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    [Fact]
    public async Task AdicionarEObterPorId_PreservaTodosOsCampos()
    {
        var l = await CriarLogradouro();
        Assert.True(l.Id > 0);
        var salvo = Assert.IsType<Logradouro>(await Logradouros.ObterPorId(l.Id));
        Assert.Equal(l.Cep.Valor, salvo.Cep.Valor);
        Assert.Equal(Nome, salvo.Nome); Assert.Equal(Sobrenome, salvo.Bairro);
        Assert.Equal(Sigla, salvo.Cidade); Assert.Equal("SC", salvo.Estado); Assert.Equal("Brasil", salvo.Pais);
    }
    [Fact]
    public async Task ObterPorId_InexistenteRetornaNulo() => Assert.Null(await Logradouros.ObterPorId(int.MaxValue));
    [Fact]
    public async Task ObterTodos_ContemRegistroInserido()
    {
        var l = await CriarLogradouro();
        Assert.Contains(await Logradouros.ObterTodos(), x => x.Id == l.Id);
    }
    [Fact]
    public async Task Atualizar_PersisteNovoCep()
    {
        var l = await CriarLogradouro();
        var novo = NovoLogradouro(l.Id);
        await Logradouros.Atualizar(novo);
        Assert.Equal(novo.Cep.Valor, (await Logradouros.ObterPorId(l.Id))!.Cep.Valor);
        Assert.Null(await Logradouros.ObterPorCep(l.Cep));
        // Atualização sem mudar valores também deve encontrar o registro (MySQL).
        await Logradouros.Atualizar(novo);
    }
    [Fact]
    public async Task Atualizar_InexistenteLancaExcecao()
    {
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => Logradouros.Atualizar(NovoLogradouro(int.MaxValue)));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }
    [Fact]
    public async Task Remover_ExcluiRegistro()
    {
        var l = await CriarLogradouro();
        Assert.True(await Logradouros.Remover(l.Id)); Assert.Null(await Logradouros.ObterPorId(l.Id));
    }
    [Fact]
    public async Task Remover_InexistenteRetornaFalse() => Assert.False(await Logradouros.Remover(int.MaxValue));
    [Fact]
    public async Task ObterPorCep_EncontradoEInexistente()
    {
        var l = await CriarLogradouro();
        Assert.Equal(l.Id, (await Logradouros.ObterPorCep(l.Cep))!.Id);
        Assert.Null(await Logradouros.ObterPorCep(Cep.Criar("00000000").ValidarMapeamento()));
    }
    [Fact]
    public async Task CepJaExiste_ConsideraIdExcluido()
    {
        var l = await CriarLogradouro();
        Assert.True(await Logradouros.CepJaExiste(l.Cep));
        Assert.False(await Logradouros.CepJaExiste(l.Cep, l.Id));
        Assert.True(await Logradouros.CepJaExiste(l.Cep, int.MaxValue));
        Assert.False(await Logradouros.CepJaExiste(Cep.Criar("00000000").ValidarMapeamento()));
    }
    [Fact]
    public async Task ObterPorCidade_FiltraESuportaMinusculas()
    {
        var l = await CriarLogradouro();
        var items = await Logradouros.ObterPorCidade(Sigla.ToLowerInvariant());
        Assert.Contains(items, x => x.Id == l.Id); Assert.All(items, x => Assert.Equal(Sigla, x.Cidade));
        Assert.Empty(await Logradouros.ObterPorCidade("Cidade inexistente"));
        Assert.Empty(await Logradouros.ObterPorCidade("' OR 1=1 --"));
    }
    [Fact]
    public async Task ObterPorBairro_ConsideraCidadeEBairro()
    {
        var l = await CriarLogradouro();
        var items = await Logradouros.ObterPorBairro(Sigla, Sobrenome);
        Assert.Contains(items, x => x.Id == l.Id);
        Assert.All(items, x => { Assert.Equal(Sigla, x.Cidade); Assert.Equal(Sobrenome, x.Bairro); });
        Assert.Empty(await Logradouros.ObterPorBairro(Sigla, "Inexistente"));
    }
    [Fact]
    public async Task Adicionar_CepDuplicadoPreservaRegistroOriginal()
    {
        var l = await CriarLogradouro();
        await Assert.ThrowsAsync<InfrastructureException>(() => Logradouros.Adicionar(NovoLogradouro(cep: l.Cep.Valor)));
        Assert.Equal(l.Id, (await Logradouros.ObterPorCep(l.Cep))!.Id);
    }
    [Fact]
    public async Task Remover_LogradouroEmUsoRespeitaChaveEstrangeira()
    {
        var aluno = await CriarAluno();
        await Assert.ThrowsAsync<InfrastructureException>(() => Logradouros.Remover(aluno.Endereco.LogradouroId));
        Assert.NotNull(await Logradouros.ObterPorId(aluno.Endereco.LogradouroId));
    }
    [Fact]
    public async Task OperacaoCanceladaNaoInsereRegistro()
    {
        var l = NovoLogradouro();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Logradouros.Adicionar(l, new CancellationToken(true)));
        Assert.False(await Logradouros.CepJaExiste(l.Cep));
    }
}
