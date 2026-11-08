// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Validation;

namespace AcademiaDoZe.Application.Tests;

public sealed class Dev14MatriculaTests : ApplicationTestBase
{
    [Theory]
    [InlineData(AppMatriculaPlano.Mensal, 1)]
    [InlineData(AppMatriculaPlano.Trimestral, 3)]
    [InlineData(AppMatriculaPlano.Semestral, 6)]
    [InlineData(AppMatriculaPlano.Anual, 12)]
    public async Task DataFimDaTelaConfereComDataPersistida(AppMatriculaPlano plano, int meses)
    {
        var dto = NovaMatricula(await CriarAluno());
        dto.Plano = plano; dto.DataInicio = new DateOnly(2028, 1, 31);
        var calculated = MatriculaCadastroValidation.CalcularDataFim(plano, dto.DataInicio);
        dto.DataFim = new DateOnly(2099, 1, 1);
        var saved = await Matriculas.AdicionarAsync(dto);
        Assert.Equal(dto.DataInicio.AddMonths(meses), calculated);
        Assert.Equal(calculated, saved.DataFim);
        Assert.Equal(calculated, (await Matriculas.ObterPorIdAsync(saved.Id))!.DataFim);
    }

    [Fact]
    public void DataFimAnualRespeitaAnoBissexto() =>
        Assert.Equal(new DateOnly(2025, 2, 28), MatriculaCadastroValidation.CalcularDataFim(AppMatriculaPlano.Anual, new DateOnly(2024, 2, 29)));

    [Fact]
    public void FormularioIncompletoRetornaVariosErros()
    {
        var dto = NovaMatricula(NovoAluno(NovoLogradouro()));
        dto.DataInicio = default; dto.Objetivo = ""; dto.Plano = (AppMatriculaPlano)99;
        var errors = MatriculaCadastroValidation.Validate(dto);
        Assert.Contains(errors, x => x.Contains("aluno"));
        Assert.Contains(errors, x => x.Contains("início"));
        Assert.Contains(errors, x => x.Contains("objetivo"));
        Assert.Contains(errors, x => x.Contains("plano"));
    }

    [Fact]
    public void FormularioDetectaFlagsInvalidosEArquivoVazio()
    {
        var dto = NovaMatricula(NovoAluno(NovoLogradouro())); dto.AlunoMatricula.Id = 1;
        dto.RestricoesMedicas = (AppMatriculaRestricoes)128;
        dto.LaudoMedico = new ArquivoDto { Conteudo = [] };
        var errors = MatriculaCadastroValidation.Validate(dto);
        Assert.Contains(errors, x => x.Contains("restrições médicas válidas"));
        Assert.Contains(errors, x => x.Contains("conteúdo"));
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(16, false)]
    [InlineData(30, false)]
    public void AvisoDeLaudoSegueARegraDeIdadeDoProjeto(int idade, bool exigido)
    {
        var dto = NovaMatricula(NovoAluno(NovoLogradouro()));
        dto.AlunoMatricula.Id = 1; dto.AlunoMatricula.DataNascimento = Hoje.AddYears(-idade);
        Assert.Equal(exigido, MatriculaCadastroValidation.ExigeLaudo(dto));
        dto.RestricoesMedicas = AppMatriculaRestricoes.Alergias;
        Assert.True(MatriculaCadastroValidation.ExigeLaudo(dto));
    }

    [Fact]
    public async Task CadastroBuscaEdicaoDeMultiplasRestricoesEExclusao()
    {
        var aluno = await CriarAluno();
        var dto = NovaMatricula(aluno);
        dto.RestricoesMedicas = AppMatriculaRestricoes.Diabetes | AppMatriculaRestricoes.PressaoAlta;
        dto.LaudoMedico = new ArquivoDto { Conteudo = [1, 2, 3, 4] };
        var saved = await Matriculas.AdicionarAsync(dto);
        var foundAluno = await Alunos.ObterPorCpfAsync(aluno.Cpf);
        var found = Assert.Single(await Matriculas.ObterPorAlunoIdAsync(foundAluno!.Id));
        Assert.Equal(aluno.Nome, found.AlunoMatricula.Nome);
        Assert.Equal(aluno.Cpf, found.AlunoMatricula.Cpf);
        Assert.Equal(aluno.DataNascimento, found.AlunoMatricula.DataNascimento);
        Assert.Equal(aluno.Foto!.Conteudo, found.AlunoMatricula.Foto!.Conteudo);
        found.RestricoesMedicas = AppMatriculaRestricoes.Alergias | AppMatriculaRestricoes.RemedioContinuo;
        found.ObservacoesRestricoes = "Cuidados de demonstração";
        await Matriculas.AtualizarAsync(found);
        var edited = (await Matriculas.ObterPorIdAsync(saved.Id))!;
        Assert.Equal(found.RestricoesMedicas, edited.RestricoesMedicas);
        Assert.Equal("Alergias, Remédio Contínuo", edited.RestricoesMedicas.GetDisplayName());
        Assert.Equal(found.LaudoMedico!.Conteudo, edited.LaudoMedico!.Conteudo);
        Assert.True(await Matriculas.RemoverAsync(edited.Id));
        Assert.Empty(await Matriculas.ObterPorAlunoIdAsync(aluno.Id));
        Assert.NotNull(await Alunos.ObterPorIdAsync(aluno.Id));
    }

    [Fact]
    public async Task RemocaoExplicitaDoLaudoPersisteQuandoOpcional()
    {
        var dto = NovaMatricula(await CriarAluno());
        dto.LaudoMedico = new ArquivoDto { Conteudo = [1, 2, 3] };
        var saved = await Matriculas.AdicionarAsync(dto);
        saved.LaudoMedico = null; saved.RemoverLaudoMedico = true;
        Assert.Null((await Matriculas.AtualizarAsync(saved)).LaudoMedico);
        Assert.Null((await Matriculas.ObterPorIdAsync(saved.Id))!.LaudoMedico);
    }

    [Fact]
    public async Task RemocaoDeLaudoObrigatorioNaoAlteraRegistro()
    {
        var dto = NovaMatricula(await CriarAluno());
        dto.RestricoesMedicas = AppMatriculaRestricoes.Alergias;
        dto.LaudoMedico = new ArquivoDto { Conteudo = [1, 2, 3] };
        var saved = await Matriculas.AdicionarAsync(dto);
        saved.LaudoMedico = null; saved.RemoverLaudoMedico = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Matriculas.AtualizarAsync(saved));
        Assert.NotNull((await Matriculas.ObterPorIdAsync(saved.Id))!.LaudoMedico);
    }

    [Fact]
    public async Task RetirarRestricoesPermiteRemoverLaudoDeAdulto()
    {
        var dto = NovaMatricula(await CriarAluno()); dto.RestricoesMedicas = AppMatriculaRestricoes.Alergias;
        dto.LaudoMedico = new ArquivoDto { Conteudo = [1, 2] };
        var saved = await Matriculas.AdicionarAsync(dto);
        saved.RestricoesMedicas = AppMatriculaRestricoes.None;
        saved.LaudoMedico = null; saved.RemoverLaudoMedico = true;
        Assert.Empty(MatriculaCadastroValidation.Validate(saved));
        var edited = await Matriculas.AtualizarAsync(saved);
        Assert.Null(edited.LaudoMedico); Assert.Equal(AppMatriculaRestricoes.None, edited.RestricoesMedicas);
    }

    [Fact]
    public async Task NaoPermiteRemoverEAnexarLaudoAoMesmoTempo()
    {
        var dto = await Matriculas.AdicionarAsync(NovaMatricula(await CriarAluno()));
        dto.LaudoMedico = new ArquivoDto { Conteudo = [1] }; dto.RemoverLaudoMedico = true;
        await Assert.ThrowsAsync<ArgumentException>(() => Matriculas.AtualizarAsync(dto));
    }
}
