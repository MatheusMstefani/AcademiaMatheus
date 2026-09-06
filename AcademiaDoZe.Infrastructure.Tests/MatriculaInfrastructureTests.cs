// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    [Fact]
    public async Task AdicionarEObterPorId_PreservaTodosOsCampos()
    {
        var aluno = await CriarAluno();
        var m = await Matriculas.Adicionar(NovaMatricula(aluno));
        Assert.True(m.Id > 0);
        var salvo = Assert.IsType<Matricula>(await Matriculas.ObterPorId(m.Id));
        Assert.Equal(aluno.Id, salvo.AlunoId); Assert.Equal(NomeCompleto, salvo.Objetivo);
        Assert.Equal(Sigla, salvo.ObservacoesRestricoes); Assert.Equal(m.DataInicio, salvo.DataInicio);
        Assert.Equal(m.DataFim, salvo.DataFim); Assert.Equal(m.Plano, salvo.Plano); Assert.Null(salvo.LaudoMedico);
    }
    [Theory]
    [InlineData(MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta)]
    [InlineData(MatriculaRestricoes.Alergias | MatriculaRestricoes.Labirintite | MatriculaRestricoes.RemedioContinuo)]
    [InlineData(MatriculaRestricoes.ProblemasRespiratorios | MatriculaRestricoes.PressaoAlta)]
    public async Task RestricoesELaudo_PersistemMultiplasEscolhas(MatriculaRestricoes flags)
    {
        var m = await Matriculas.Adicionar(NovaMatricula(await CriarAluno(), restricoes: flags));
        var salvo = (await Matriculas.ObterPorId(m.Id))!;
        Assert.Equal(flags, salvo.RestricoesMedicas);
        Assert.Equal(m.LaudoMedico!.Conteudo, salvo.LaudoMedico!.Conteudo);
        Assert.Equal(Sigla, salvo.ObservacoesRestricoes);
    }
    [Fact]
    public async Task ObterPorId_InexistenteRetornaNulo() => Assert.Null(await Matriculas.ObterPorId(int.MaxValue));
    [Fact]
    public async Task ObterTodos_ContemRegistroInserido()
    {
        var m = await Matriculas.Adicionar(NovaMatricula(await CriarAluno()));
        Assert.Contains(await Matriculas.ObterTodos(), x => x.Id == m.Id);
    }
    [Fact]
    public async Task Atualizar_PersistePlanoDatasERestricoes()
    {
        var aluno = await CriarAluno();
        var m = await Matriculas.Adicionar(NovaMatricula(aluno));
        var novo = NovaMatricula(aluno, m.Id, MatriculaPlano.Anual, restricoes: MatriculaRestricoes.Alergias);
        await Matriculas.Atualizar(novo);
        var salvo = (await Matriculas.ObterPorId(m.Id))!;
        Assert.Equal(MatriculaPlano.Anual, salvo.Plano); Assert.Equal(Hoje.AddYears(1), salvo.DataFim);
        Assert.Equal(MatriculaRestricoes.Alergias, salvo.RestricoesMedicas);
        Assert.Equal(novo.LaudoMedico!.Conteudo, salvo.LaudoMedico!.Conteudo);
        Assert.Equal(NomeCompleto, salvo.Objetivo); Assert.Equal(Sigla, salvo.ObservacoesRestricoes);
        await Matriculas.Atualizar(novo);
    }
    [Fact]
    public async Task Atualizar_InexistenteLancaExcecao()
    {
        var m = NovaMatricula(await CriarAluno(), int.MaxValue);
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => Matriculas.Atualizar(m));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }
    [Fact]
    public async Task Remover_ExcluiRegistro()
    {
        var m = await Matriculas.Adicionar(NovaMatricula(await CriarAluno()));
        Assert.True(await Matriculas.Remover(m.Id)); Assert.Null(await Matriculas.ObterPorId(m.Id));
    }
    [Fact]
    public async Task Remover_InexistenteRetornaFalse() => Assert.False(await Matriculas.Remover(int.MaxValue));
    [Fact]
    public async Task ObterPorAluno_FiltraCorretamente()
    {
        var a = await CriarAluno();
        var m = await Matriculas.Adicionar(NovaMatricula(a));
        await Matriculas.Adicionar(NovaMatricula(await CriarAluno()));
        var items = await Matriculas.ObterPorAluno(a.Id);
        Assert.Equal(m.Id, Assert.Single(items).Id);
        Assert.Empty(await Matriculas.ObterPorAluno(int.MaxValue));
    }
    [Fact]
    public async Task ObterAtivaEPossuiAtiva_ExcluemVencidasEFuturas()
    {
        var a = await CriarAluno();
        Assert.False(await Matriculas.PossuiMatriculaAtiva(a.Id));
        await Matriculas.Adicionar(NovaMatricula(a, inicio: Hoje.AddMonths(-2)));
        await Matriculas.Adicionar(NovaMatricula(a, inicio: Hoje.AddDays(1)));
        Assert.Null(await Matriculas.ObterMatriculaAtivaPorAluno(a.Id));
        var m = await Matriculas.Adicionar(NovaMatricula(a));
        Assert.True(await Matriculas.PossuiMatriculaAtiva(a.Id));
        Assert.Equal(m.Id, (await Matriculas.ObterMatriculaAtivaPorAluno(a.Id))!.Id);
        Assert.Equal(m.Id, Assert.Single(await Matriculas.ObterAtivas(a.Id)).Id);
        Assert.Contains(await Matriculas.ObterAtivas(), x => x.Id == m.Id);
    }
    [Fact]
    public async Task VencendoEmDias_RespeitaLimitesDaConsulta()
    {
        var aluno = await CriarAluno();
        var proxima = await Matriculas.Adicionar(NovaMatricula(aluno, inicio: Hoje.AddDays(-25)));
        var distante = await Matriculas.Adicionar(NovaMatricula(aluno, plano: MatriculaPlano.Anual));
        var vencida = await Matriculas.Adicionar(NovaMatricula(aluno, inicio: Hoje.AddMonths(-3)));
        var items = await Matriculas.ObterVencendoEmDias(10);
        Assert.Contains(items, x => x.Id == proxima.Id);
        Assert.DoesNotContain(items, x => x.Id == distante.Id || x.Id == vencida.Id);
        Assert.All(items, x => Assert.InRange(x.DataFim, Hoje, Hoje.AddDays(10)));
    }
    [Theory]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public async Task ObterPorPlano_FiltraEPreservaDataFim(MatriculaPlano plano, int meses)
    {
        var m = await Matriculas.Adicionar(NovaMatricula(await CriarAluno(), plano: plano));
        var items = await Matriculas.ObterPorPlano(plano);
        Assert.Contains(items, x => x.Id == m.Id);
        Assert.All(items, x => Assert.Equal(plano, x.Plano));
        Assert.Equal(Hoje.AddMonths(meses), (await Matriculas.ObterPorId(m.Id))!.DataFim);
    }
    [Fact]
    public async Task RemoverAluno_ExcluiMatriculasEmCascata()
    {
        var a = await CriarAluno();
        var m = await Matriculas.Adicionar(NovaMatricula(a));
        Assert.True(await Alunos.Remover(a.Id)); Assert.Null(await Matriculas.ObterPorId(m.Id));
    }
    [Fact]
    public async Task Adicionar_AlunoInexistenteRespeitaChaveEstrangeira()
    {
        var a = NovoAluno(await CriarLogradouro(), int.MaxValue);
        await Assert.ThrowsAsync<InfrastructureException>(() => Matriculas.Adicionar(NovaMatricula(a)));
    }
    [Fact]
    public async Task Atualizar_PermiteRemoverLaudoERestricoes()
    {
        var a = await CriarAluno();
        var m = await Matriculas.Adicionar(NovaMatricula(a, restricoes: MatriculaRestricoes.Alergias));
        await Matriculas.Atualizar(NovaMatricula(a, m.Id));
        var salvo = (await Matriculas.ObterPorId(m.Id))!;
        Assert.Null(salvo.LaudoMedico); Assert.Equal(MatriculaRestricoes.None, salvo.RestricoesMedicas);
    }
}
