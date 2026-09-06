// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    [Fact]
    public async Task AdicionarEObterPorId_PreservaDadosEEndereco()
    {
        var e = await CriarAluno();
        Assert.True(e.Id > 0);
        var salvo = Assert.IsType<Aluno>(await Alunos.ObterPorId(e.Id));
        Assert.Equal(Nome, salvo.Nome); Assert.Equal(Sobrenome, salvo.Endereco.Complemento);
        Assert.Equal(e.Cpf.Valor, salvo.Cpf.Valor); Assert.Equal(e.Email.Valor, salvo.Email.Valor);
        Assert.Equal(e.Telefone.Valor, salvo.Telefone.Valor);
        Assert.Equal(e.DataNascimento, salvo.DataNascimento);
        Assert.Equal(e.Endereco.LogradouroId, salvo.Endereco.LogradouroId);
        Assert.Equal(e.Endereco.Numero, salvo.Endereco.Numero);
        Assert.Equal(e.Foto.Conteudo, salvo.Foto.Conteudo);
        Assert.Equal(Sigla + "123!", salvo.Senha.Valor);
        
    }
    [Fact]
    public async Task ObterPorId_InexistenteRetornaNulo() => Assert.Null(await Alunos.ObterPorId(int.MaxValue));
    [Fact]
    public async Task ObterTodos_ContemRegistroInserido()
    {
        var e = await CriarAluno();
        Assert.Contains(await Alunos.ObterTodos(), x => x.Id == e.Id);
    }
    [Fact]
    public async Task Atualizar_PersisteAlteracoes()
    {
        var e = await CriarAluno();
        var outroEndereco = await CriarLogradouro();
        var novo = NovoAluno(outroEndereco, e.Id, numero: "250");
        await Alunos.Atualizar(novo);
        var salvo = (await Alunos.ObterPorId(e.Id))!;
        Assert.Equal(novo.Cpf.Valor, salvo.Cpf.Valor);
        Assert.Equal(novo.Email.Valor, salvo.Email.Valor);
        Assert.Equal(outroEndereco.Id, salvo.Endereco.LogradouroId);
        Assert.Equal("250", salvo.Endereco.Numero);
        Assert.Equal(Nome, salvo.Nome); Assert.Equal(Sobrenome, salvo.Endereco.Complemento);
        Assert.Null(await Alunos.ObterPorCpf(e.Cpf));
        await Alunos.Atualizar(novo);
    }
    [Fact]
    public async Task Atualizar_InexistenteLancaExcecao()
    {
        var e = NovoAluno(await CriarLogradouro(), int.MaxValue);
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => Alunos.Atualizar(e));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }
    [Fact]
    public async Task Remover_ExcluiRegistro()
    {
        var e = await CriarAluno();
        Assert.True(await Alunos.Remover(e.Id)); Assert.Null(await Alunos.ObterPorId(e.Id));
    }
    [Fact]
    public async Task Remover_InexistenteRetornaFalse() => Assert.False(await Alunos.Remover(int.MaxValue));
    [Fact]
    public async Task ObterPorCpf_EncontradoEInexistente()
    {
        var e = await CriarAluno();
        Assert.Equal(e.Id, (await Alunos.ObterPorCpf(e.Cpf))!.Id);
        Assert.Null(await Alunos.ObterPorCpf(Cpf.Criar(GerarCpf()).ValidarMapeamento()));
    }
    [Fact]
    public async Task ObterPorEmail_EncontradoEInexistente()
    {
        var e = await CriarAluno();
        Assert.Equal(e.Id, (await Alunos.ObterPorEmail(e.Email))!.Id);
        Assert.Null(await Alunos.ObterPorEmail(Email.Criar(GerarEmail()).ValidarMapeamento()));
    }
    [Fact]
    public async Task CpfJaExiste_ConsideraIdExcluido()
    {
        var e = await CriarAluno();
        Assert.True(await Alunos.CpfJaExiste(e.Cpf));
        Assert.False(await Alunos.CpfJaExiste(e.Cpf, e.Id));
        Assert.True(await Alunos.CpfJaExiste(e.Cpf, int.MaxValue));
        Assert.False(await Alunos.CpfJaExiste(Cpf.Criar(GerarCpf()).ValidarMapeamento()));
    }
    [Fact]
    public async Task EmailJaExiste_ConsideraIdExcluido()
    {
        var e = await CriarAluno();
        Assert.True(await Alunos.EmailJaExiste(e.Email));
        Assert.False(await Alunos.EmailJaExiste(e.Email, e.Id));
        Assert.True(await Alunos.EmailJaExiste(e.Email, int.MaxValue));
        Assert.False(await Alunos.EmailJaExiste(Email.Criar(GerarEmail()).ValidarMapeamento()));
    }
    [Fact]
    public async Task TrocarSenha_PersisteESinalizaInexistente()
    {
        var e = await CriarAluno();
        var senha = Senha.Criar(Sigla + "456!").ValidarMapeamento();
        Assert.True(await Alunos.TrocarSenha(e.Id, senha));
        Assert.Equal(senha.Valor, (await Alunos.ObterPorId(e.Id))!.Senha.Valor);
        Assert.True(await Alunos.TrocarSenha(e.Id, senha));
        Assert.False(await Alunos.TrocarSenha(int.MaxValue, senha));
    }
    [Fact]
    public async Task Adicionar_CpfDuplicadoLancaExcecao()
    {
        var e = await CriarAluno();
        var duplicado = NovoAluno(await CriarLogradouro(), cpf: e.Cpf.Valor);
        await Assert.ThrowsAsync<InfrastructureException>(() => Alunos.Adicionar(duplicado));
        Assert.Equal(e.Id, (await Alunos.ObterPorCpf(e.Cpf))!.Id);
    }
    [Fact]
    public async Task Adicionar_EmailDuplicadoLancaExcecao()
    {
        var e = await CriarAluno();
        var duplicado = NovoAluno(await CriarLogradouro(), email: e.Email.Valor);
        await Assert.ThrowsAsync<InfrastructureException>(() => Alunos.Adicionar(duplicado));
    }
    [Fact]
    public async Task Adicionar_LogradouroInexistenteRespeitaChaveEstrangeira()
    {
        var e = NovoAluno(NovoLogradouro(int.MaxValue));
        await Assert.ThrowsAsync<InfrastructureException>(() => Alunos.Adicionar(e));
    }
    [Fact]
    public async Task ObterPorNome_FiltraTrechoSemInterpretarSql()
    {
        var e = await CriarAluno();
        var items = await Alunos.ObterPorNome("mathe");
        Assert.Contains(items, x => x.Id == e.Id); Assert.All(items, x => Assert.Equal(Nome, x.Nome));
        Assert.Empty(await Alunos.ObterPorNome("' OR 1=1 --"));
        Assert.Empty(await Alunos.ObterPorNome("%"));
    }
}

