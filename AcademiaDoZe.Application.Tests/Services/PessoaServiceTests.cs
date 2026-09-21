// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.Tests.Services;

public class PessoaServiceTests : ApplicationTestBase
{
    [Fact]
    public async Task Aluno_CrudConsultasESenhasNaoExpostas()
    {
        var input = NovoAluno(await Logradouros.AdicionarAsync(NovoLogradouro()));
        var dto = await Alunos.AdicionarAsync(input);
        Assert.Equal("MinhaSenha123!", input.Senha);
        Assert.Null(dto.Senha);
        var repo = Services.GetRequiredService<IAlunoRepository>();
        var salvo = await repo.ObterPorId(dto.Id);
        var hash = salvo!.Senha.Valor;
        Assert.True(PasswordHasher.Verify(input.Senha, hash));
        Assert.NotEqual(input.Senha, hash);
        Assert.Equal(dto.Id, (await Alunos.ObterPorCpfAsync(input.Cpf))!.Id);
        Assert.NotNull((await Alunos.ObterPorEmailAsync(input.Email!))!.Endereco);
        Assert.Single(await Alunos.ObterPorNomeAsync("matheus"));
        Assert.Single(await Alunos.ObterTodosAsync());
        Assert.True(await Alunos.CpfJaExisteAsync(input.Cpf));
        Assert.True(await Alunos.EmailJaExisteAsync(input.Email!));
        Assert.False(await Alunos.CpfJaExisteAsync(input.Cpf, dto.Id));
        Assert.False(await Alunos.EmailJaExisteAsync(input.Email!, dto.Id));
        dto.Nome = "Matheus Atualizado";
        dto.Foto = null;
        dto.Endereco = null;
        var atualizado = await Alunos.AtualizarAsync(dto);
        Assert.Equal(dto.Nome, atualizado.Nome);
        Assert.NotNull(atualizado.Endereco);
        Assert.NotNull(atualizado.Foto);
        Assert.Equal(hash, (await repo.ObterPorId(dto.Id))!.Senha.Valor);
        Assert.True(await Alunos.TrocarSenhaAsync(dto.Id, "NovaSenha456!"));
        var novoHash = (await repo.ObterPorId(dto.Id))!.Senha.Valor;
        Assert.True(PasswordHasher.Verify("NovaSenha456!", novoHash));
        Assert.False(PasswordHasher.Verify(input.Senha, novoHash));
        atualizado.Senha = "EdicaoSenha789!";
        Assert.Null((await Alunos.AtualizarAsync(atualizado)).Senha);
        Assert.True(PasswordHasher.Verify(atualizado.Senha, (await repo.ObterPorId(dto.Id))!.Senha.Valor));
        Assert.True(await Alunos.RemoverAsync(dto.Id));
        Assert.False(await Alunos.RemoverAsync(dto.Id));
        Assert.Null(await Alunos.ObterPorIdAsync(dto.Id));
    }

    [Fact]
    public async Task Colaborador_CrudConsultasETrocaParaEnumsZero()
    {
        var input = NovoColaborador(await Logradouros.AdicionarAsync(NovoLogradouro()));
        var dto = await Colaboradores.AdicionarAsync(input);
        Assert.Equal("MinhaSenha123!", input.Senha);
        Assert.Null(dto.Senha);
        var repo = Services.GetRequiredService<IColaboradorRepository>();
        var hash = (await repo.ObterPorId(dto.Id))!.Senha.Valor;
        Assert.True(PasswordHasher.Verify(input.Senha, hash));
        Assert.Equal(dto.Id, (await Colaboradores.ObterPorCpfAsync(input.Cpf))!.Id);
        Assert.NotNull((await Colaboradores.ObterPorEmailAsync(input.Email!))!.Endereco);
        Assert.Single(await Colaboradores.ObterTodosAsync());
        Assert.Single(await Colaboradores.ObterPorTipoAsync(AppColaboradorTipo.Instrutor));
        Assert.Single(await Colaboradores.ObterPorVinculoAsync(AppColaboradorVinculo.Estagio));
        Assert.True(await Colaboradores.CpfJaExisteAsync(input.Cpf));
        Assert.True(await Colaboradores.EmailJaExisteAsync(input.Email!));
        Assert.False(await Colaboradores.CpfJaExisteAsync(input.Cpf, dto.Id));
        Assert.False(await Colaboradores.EmailJaExisteAsync(input.Email!, dto.Id));
        dto.Tipo = AppColaboradorTipo.Administrador;
        dto.Vinculo = AppColaboradorVinculo.CLT;
        var atualizado = await Colaboradores.AtualizarAsync(dto);
        Assert.Equal(AppColaboradorTipo.Administrador, atualizado.Tipo);
        Assert.Equal(AppColaboradorVinculo.CLT, atualizado.Vinculo);
        Assert.Equal(hash, (await repo.ObterPorId(dto.Id))!.Senha.Valor);
        Assert.True(await Colaboradores.TrocarSenhaAsync(dto.Id, "NovaSenha456!"));
        Assert.True(PasswordHasher.Verify("NovaSenha456!", (await repo.ObterPorId(dto.Id))!.Senha.Valor));
        dto.Senha = "EdicaoSenha789!";
        Assert.Null((await Colaboradores.AtualizarAsync(dto)).Senha);
        Assert.True(PasswordHasher.Verify(dto.Senha, (await repo.ObterPorId(dto.Id))!.Senha.Valor));
        Assert.True(await Colaboradores.RemoverAsync(dto.Id));
        Assert.False(await Colaboradores.RemoverAsync(dto.Id));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task DuplicidadeCpfOuEmail_NaoInsere(bool colaborador, bool emailDuplicado)
    {
        var log = await Logradouros.AdicionarAsync(NovoLogradouro());
        if (colaborador)
        {
            await Colaboradores.AdicionarAsync(NovoColaborador(log));
            var dto = NovoColaborador(log);
            if (emailDuplicado) dto.Cpf = "11144477735";
            else dto.Email = "outra@example.com";
            await Assert.ThrowsAsync<InvalidOperationException>(() => Colaboradores.AdicionarAsync(dto));
            Assert.Single(await Colaboradores.ObterTodosAsync());
        }
        else
        {
            await Alunos.AdicionarAsync(NovoAluno(log));
            var dto = NovoAluno(log);
            if (emailDuplicado) dto.Cpf = "11144477735";
            else dto.Email = "outra@example.com";
            await Assert.ThrowsAsync<InvalidOperationException>(() => Alunos.AdicionarAsync(dto));
            Assert.Single(await Alunos.ObterTodosAsync());
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("somente minusculas")]
    public async Task SenhaInvalida_RejeitadaAntesDoHash(string senha)
    {
        var log = await Logradouros.AdicionarAsync(NovoLogradouro());
        var aluno = NovoAluno(log);
        aluno.Senha = senha;
        await Assert.ThrowsAnyAsync<Exception>(() => Alunos.AdicionarAsync(aluno));
        var colaborador = NovoColaborador(log);
        colaborador.Senha = senha;
        await Assert.ThrowsAnyAsync<Exception>(() => Colaboradores.AdicionarAsync(colaborador));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Alunos.TrocarSenhaAsync(1, senha));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Colaboradores.TrocarSenhaAsync(1, senha));
        Assert.Empty(await Alunos.ObterTodosAsync());
        Assert.Empty(await Colaboradores.ObterTodosAsync());
    }

    [Fact]
    public async Task EnderecoInexistenteEArquivoInvalido_NaoInsere()
    {
        var log = NovoLogradouro();
        log.Id = 999;
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Alunos.AdicionarAsync(NovoAluno(log)));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Colaboradores.AdicionarAsync(NovoColaborador(log)));
        log = await Logradouros.AdicionarAsync(NovoLogradouro());
        var aluno = NovoAluno(log);
        aluno.Foto = new ArquivoDto { Conteudo = [] };
        await Assert.ThrowsAsync<InvalidOperationException>(() => Alunos.AdicionarAsync(aluno));
        var colaborador = NovoColaborador(log);
        colaborador.Foto = new ArquivoDto { Conteudo = [] };
        await Assert.ThrowsAsync<InvalidOperationException>(() => Colaboradores.AdicionarAsync(colaborador));
    }

    [Fact]
    public async Task CpfImutavel_ImpedeAlteracaoSilenciosa()
    {
        var aluno = await CriarAluno();
        aluno.Cpf = "11144477735";
        await Assert.ThrowsAsync<InvalidOperationException>(() => Alunos.AtualizarAsync(aluno));
        Assert.Equal("52998224725", (await Alunos.ObterPorIdAsync(aluno.Id))!.Cpf);
    }

    [Fact]
    public async Task ConsultasInvalidas_NaoChegamAoBanco()
    {
        Assert.False(await Alunos.CpfJaExisteAsync("123"));
        Assert.False(await Colaboradores.EmailJaExisteAsync("sem-arroba"));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Alunos.ObterPorCpfAsync("123"));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Colaboradores.ObterPorEmailAsync("x"));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Alunos.ObterPorNomeAsync(""));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Colaboradores.ObterPorTipoAsync((AppColaboradorTipo)99));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Colaboradores.ObterPorVinculoAsync((AppColaboradorVinculo)99));
    }
}
