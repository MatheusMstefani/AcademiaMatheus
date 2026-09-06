// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Tests;

public class ColaboradorInfrastructureTests : TestBase
{
    [Fact]
    public async Task AdicionarEObterPorId_PreservaDadosEEndereco()
    {
        var e = await CriarColaborador();
        Assert.True(e.Id > 0);
        var salvo = Assert.IsType<Colaborador>(await Colaboradores.ObterPorId(e.Id));
        Assert.Equal(Nome, salvo.Nome); Assert.Equal(Sobrenome, salvo.Endereco.Complemento);
        Assert.Equal(e.Cpf.Valor, salvo.Cpf.Valor); Assert.Equal(e.Email.Valor, salvo.Email.Valor);
        Assert.Equal(e.Telefone.Valor, salvo.Telefone.Valor);
        Assert.Equal(e.DataNascimento, salvo.DataNascimento);
        Assert.Equal(e.Endereco.LogradouroId, salvo.Endereco.LogradouroId);
        Assert.Equal(e.Endereco.Numero, salvo.Endereco.Numero);
        Assert.Equal(e.Foto.Conteudo, salvo.Foto.Conteudo);
        Assert.Equal(Sigla + "123!", salvo.Senha.Valor);
        Assert.Equal(e.Tipo, salvo.Tipo); Assert.Equal(e.Vinculo, salvo.Vinculo); Assert.Equal(e.DataAdmissao, salvo.DataAdmissao);
    }
    [Fact]
    public async Task ObterPorId_InexistenteRetornaNulo() => Assert.Null(await Colaboradores.ObterPorId(int.MaxValue));
    [Fact]
    public async Task ObterTodos_ContemRegistroInserido()
    {
        var e = await CriarColaborador();
        Assert.Contains(await Colaboradores.ObterTodos(), x => x.Id == e.Id);
    }
    [Fact]
    public async Task Atualizar_PersisteAlteracoes()
    {
        var e = await CriarColaborador();
        var outroEndereco = await CriarLogradouro();
        var novo = NovoColaborador(outroEndereco, e.Id, numero: "250");
        await Colaboradores.Atualizar(novo);
        var salvo = (await Colaboradores.ObterPorId(e.Id))!;
        Assert.Equal(novo.Cpf.Valor, salvo.Cpf.Valor);
        Assert.Equal(novo.Email.Valor, salvo.Email.Valor);
        Assert.Equal(outroEndereco.Id, salvo.Endereco.LogradouroId);
        Assert.Equal("250", salvo.Endereco.Numero);
        Assert.Equal(Nome, salvo.Nome); Assert.Equal(Sobrenome, salvo.Endereco.Complemento);
        Assert.Null(await Colaboradores.ObterPorCpf(e.Cpf));
        await Colaboradores.Atualizar(novo);
    }
    [Fact]
    public async Task Atualizar_InexistenteLancaExcecao()
    {
        var e = NovoColaborador(await CriarLogradouro(), int.MaxValue);
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => Colaboradores.Atualizar(e));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }
    [Fact]
    public async Task Remover_ExcluiRegistro()
    {
        var e = await CriarColaborador();
        Assert.True(await Colaboradores.Remover(e.Id)); Assert.Null(await Colaboradores.ObterPorId(e.Id));
    }
    [Fact]
    public async Task Remover_InexistenteRetornaFalse() => Assert.False(await Colaboradores.Remover(int.MaxValue));
    [Fact]
    public async Task ObterPorCpf_EncontradoEInexistente()
    {
        var e = await CriarColaborador();
        Assert.Equal(e.Id, (await Colaboradores.ObterPorCpf(e.Cpf))!.Id);
        Assert.Null(await Colaboradores.ObterPorCpf(Cpf.Criar(GerarCpf()).ValidarMapeamento()));
    }
    [Fact]
    public async Task ObterPorEmail_EncontradoEInexistente()
    {
        var e = await CriarColaborador();
        Assert.Equal(e.Id, (await Colaboradores.ObterPorEmail(e.Email))!.Id);
        Assert.Null(await Colaboradores.ObterPorEmail(Email.Criar(GerarEmail()).ValidarMapeamento()));
    }
    [Fact]
    public async Task CpfJaExiste_ConsideraIdExcluido()
    {
        var e = await CriarColaborador();
        Assert.True(await Colaboradores.CpfJaExiste(e.Cpf));
        Assert.False(await Colaboradores.CpfJaExiste(e.Cpf, e.Id));
        Assert.True(await Colaboradores.CpfJaExiste(e.Cpf, int.MaxValue));
        Assert.False(await Colaboradores.CpfJaExiste(Cpf.Criar(GerarCpf()).ValidarMapeamento()));
    }
    [Fact]
    public async Task EmailJaExiste_ConsideraIdExcluido()
    {
        var e = await CriarColaborador();
        Assert.True(await Colaboradores.EmailJaExiste(e.Email));
        Assert.False(await Colaboradores.EmailJaExiste(e.Email, e.Id));
        Assert.True(await Colaboradores.EmailJaExiste(e.Email, int.MaxValue));
        Assert.False(await Colaboradores.EmailJaExiste(Email.Criar(GerarEmail()).ValidarMapeamento()));
    }
    [Fact]
    public async Task TrocarSenha_PersisteESinalizaInexistente()
    {
        var e = await CriarColaborador();
        var senha = Senha.Criar(Sigla + "456!").ValidarMapeamento();
        Assert.True(await Colaboradores.TrocarSenha(e.Id, senha));
        Assert.Equal(senha.Valor, (await Colaboradores.ObterPorId(e.Id))!.Senha.Valor);
        Assert.True(await Colaboradores.TrocarSenha(e.Id, senha));
        Assert.False(await Colaboradores.TrocarSenha(int.MaxValue, senha));
    }
    [Fact]
    public async Task Adicionar_CpfDuplicadoLancaExcecao()
    {
        var e = await CriarColaborador();
        var duplicado = NovoColaborador(await CriarLogradouro(), cpf: e.Cpf.Valor);
        await Assert.ThrowsAsync<InfrastructureException>(() => Colaboradores.Adicionar(duplicado));
        Assert.Equal(e.Id, (await Colaboradores.ObterPorCpf(e.Cpf))!.Id);
    }
    [Fact]
    public async Task Adicionar_EmailDuplicadoLancaExcecao()
    {
        var e = await CriarColaborador();
        var duplicado = NovoColaborador(await CriarLogradouro(), email: e.Email.Valor);
        await Assert.ThrowsAsync<InfrastructureException>(() => Colaboradores.Adicionar(duplicado));
    }
    [Fact]
    public async Task Adicionar_LogradouroInexistenteRespeitaChaveEstrangeira()
    {
        var e = NovoColaborador(NovoLogradouro(int.MaxValue));
        await Assert.ThrowsAsync<InfrastructureException>(() => Colaboradores.Adicionar(e));
    }
    [Theory]
    [InlineData(ColaboradorTipo.Instrutor)]
    [InlineData(ColaboradorTipo.Atendente)]
    [InlineData(ColaboradorTipo.Administrador)]
    public async Task ObterPorTipo_FiltraTodosOsTipos(ColaboradorTipo tipo)
    {
        var e = await Colaboradores.Adicionar(NovoColaborador(await CriarLogradouro(), tipo: tipo));
        var items = await Colaboradores.ObterPorTipo(tipo);
        Assert.Contains(items, x => x.Id == e.Id); Assert.All(items, x => Assert.Equal(tipo, x.Tipo));
    }
    [Theory]
    [InlineData(ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorVinculo.Estagio)]
    public async Task ObterPorVinculo_FiltraTodosOsVinculos(ColaboradorVinculo vinculo)
    {
        var e = await Colaboradores.Adicionar(NovoColaborador(await CriarLogradouro(), vinculo: vinculo));
        var items = await Colaboradores.ObterPorVinculo(vinculo);
        Assert.Contains(items, x => x.Id == e.Id); Assert.All(items, x => Assert.Equal(vinculo, x.Vinculo));
    }
}

