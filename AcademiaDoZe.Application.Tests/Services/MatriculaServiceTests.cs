// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.Tests.Services;

public class MatriculaServiceTests : ApplicationTestBase
{
    [Fact]
    public async Task CrudConsultasECalculoDeDatas_Funcionam()
    {
        var aluno = await CriarAluno();
        Assert.Null(aluno.Senha);
        var input = NovaMatricula(aluno);
        input.DataFim = Hoje.AddYears(20); // Não confiar em uma data fim enviada pela interface.
        var dto = await Matriculas.AdicionarAsync(input);
        Assert.Equal(Hoje.AddMonths(1), dto.DataFim);
        Assert.Null(dto.AlunoMatricula.Senha);
        Assert.Equal(dto.Id, (await Matriculas.ObterPorIdAsync(dto.Id))!.Id);
        Assert.Single(await Matriculas.ObterTodasAsync());
        Assert.Single(await Matriculas.ObterPorAlunoIdAsync(aluno.Id));
        Assert.Single(await Matriculas.ObterAtivasAsync(aluno.Id));
        Assert.Single(await Matriculas.ObterPorPlanoAsync(AppMatriculaPlano.Mensal));
        Assert.Single(await Matriculas.ObterVencendoEmDiasAsync(32));
        Assert.True(await Matriculas.PossuiMatriculaAtivaAsync(aluno.Id));
        Assert.Equal(dto.Id, (await Matriculas.ObterMatriculaAtivaPorAlunoAsync(aluno.Id))!.Id);
        dto.Plano = AppMatriculaPlano.Anual;
        var anual = await Matriculas.AtualizarAsync(dto);
        Assert.Equal(Hoje.AddYears(1), anual.DataFim);
        anual.Plano = AppMatriculaPlano.Mensal;
        Assert.Equal(Hoje.AddMonths(1), (await Matriculas.AtualizarAsync(anual)).DataFim);
        Assert.True(await Matriculas.RemoverAsync(dto.Id));
        Assert.False(await Matriculas.RemoverAsync(dto.Id));
        Assert.Null(await Matriculas.ObterPorIdAsync(dto.Id));
        Assert.False(await Matriculas.PossuiMatriculaAtivaAsync(aluno.Id));
    }

    [Fact]
    public async Task SegundaMatriculaAtiva_EBloqueada()
    {
        var aluno = await CriarAluno();
        await Matriculas.AdicionarAsync(NovaMatricula(aluno));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AdicionarAsync(NovaMatricula(aluno)));
        Assert.Single(await Matriculas.ObterTodasAsync());
    }

    [Fact]
    public async Task Edicao_NaoPodeReativarMatriculaSeOutraEstaAtiva()
    {
        var aluno = await CriarAluno();
        var antiga = NovaMatricula(aluno);
        antiga.DataInicio = Hoje.AddYears(-2);
        var salva = await Matriculas.AdicionarAsync(antiga);
        await Matriculas.AdicionarAsync(NovaMatricula(aluno));
        salva.DataInicio = Hoje;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AtualizarAsync(salva));
        Assert.Single(await Matriculas.ObterAtivasAsync(aluno.Id));
    }

    [Theory]
    [InlineData(15, false)]
    [InlineData(16, true)]
    public async Task MenorDe16_ExigeLaudoUsandoDadosDoBanco(int idade, bool permitido)
    {
        var input = NovoAluno(await Logradouros.AdicionarAsync(NovoLogradouro()));
        input.DataNascimento = Hoje.AddYears(-idade);
        var aluno = await Alunos.AdicionarAsync(input);
        aluno.DataNascimento = Hoje.AddYears(-40); // O DTO não pode burlar a idade persistida.
        var matricula = NovaMatricula(aluno);
        if (permitido)
            Assert.True((await Matriculas.AdicionarAsync(matricula)).Id > 0);
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AdicionarAsync(matricula));
            matricula.LaudoMedico = new ArquivoDto { Conteudo = [1, 2] };
            Assert.True((await Matriculas.AdicionarAsync(matricula)).Id > 0);
        }
    }

    [Fact]
    public async Task Restricoes_ExigemLaudoEPermitemVoltarParaNone()
    {
        var matricula = NovaMatricula(await CriarAluno());
        matricula.RestricoesMedicas = AppMatriculaRestricoes.Diabetes | AppMatriculaRestricoes.PressaoAlta;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AdicionarAsync(matricula));
        matricula.LaudoMedico = new ArquivoDto { Conteudo = [1, 2] };
        var dto = await Matriculas.AdicionarAsync(matricula);
        dto.RestricoesMedicas = AppMatriculaRestricoes.None;
        dto.LaudoMedico = null; // Não substituir o documento existente.
        var atualizado = await Matriculas.AtualizarAsync(dto);
        Assert.Equal(AppMatriculaRestricoes.None, atualizado.RestricoesMedicas);
        Assert.NotNull(atualizado.LaudoMedico);
    }

    [Fact]
    public async Task ArquivoInvalido_ERejeitadoMesmoQuandoLaudoOpcional()
    {
        var dto = NovaMatricula(await CriarAluno());
        dto.LaudoMedico = new ArquivoDto { Conteudo = [] };
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AdicionarAsync(dto));
        Assert.Empty(await Matriculas.ObterTodasAsync());
    }

    [Fact]
    public async Task EditarParaRestricaoSemLaudo_EBloqueado()
    {
        var dto = await Matriculas.AdicionarAsync(NovaMatricula(await CriarAluno()));
        dto.RestricoesMedicas = AppMatriculaRestricoes.Alergias;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AtualizarAsync(dto));
        Assert.Equal(AppMatriculaRestricoes.None, (await Matriculas.ObterPorIdAsync(dto.Id))!.RestricoesMedicas);
    }

    [Fact]
    public async Task AlunoInexistenteENovaAssociacao_SaoRejeitados()
    {
        var aluno = await CriarAluno();
        var dto = await Matriculas.AdicionarAsync(NovaMatricula(aluno));
        dto.AlunoMatricula.Id = 999;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AtualizarAsync(dto));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AdicionarAsync(NovaMatricula(dto.AlunoMatricula)));
    }

    [Fact]
    public async Task FiltrosInvalidos_SaoRejeitados()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Matriculas.ObterVencendoEmDiasAsync(-1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Matriculas.ObterPorPlanoAsync((AppMatriculaPlano)99));
        Assert.Null(await Matriculas.ObterMatriculaAtivaPorAlunoAsync(999));
    }
}
