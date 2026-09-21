// Matheus Marques Stefani
namespace AcademiaDoZe.Application.Tests.Services;

public class LogradouroServiceTests : ApplicationTestBase
{
    [Fact]
    public async Task CrudEConsultas_FuncionamComSQLite()
    {
        var dto = await Logradouros.AdicionarAsync(NovoLogradouro());
        Assert.True(dto.Id > 0);
        Assert.Equal("Rua de Teste", dto.Nome);
        Assert.Equal("SC", dto.Estado);
        Assert.Equal(dto.Id, (await Logradouros.ObterPorIdAsync(dto.Id))!.Id);
        Assert.Equal(dto.Id, (await Logradouros.ObterPorCepAsync("88010-000"))!.Id);
        Assert.Single(await Logradouros.ObterTodosAsync());
        Assert.Single(await Logradouros.ObterPorCidadeAsync(" Florianópolis "));
        Assert.Single(await Logradouros.ObterPorBairroAsync("Florianópolis", "Centro"));
        Assert.True(await Logradouros.CepJaExisteAsync(dto.Cep));
        Assert.False(await Logradouros.CepJaExisteAsync(dto.Cep, dto.Id));
        dto.Nome = "Rua Nova";
        Assert.Equal("Rua Nova", (await Logradouros.AtualizarAsync(dto)).Nome);
        Assert.True(await Logradouros.RemoverAsync(dto.Id));
        Assert.False(await Logradouros.RemoverAsync(dto.Id));
        Assert.Null(await Logradouros.ObterPorIdAsync(dto.Id));
    }

    [Fact]
    public async Task CepDuplicado_RejeitaCriacaoEEdicao()
    {
        await Logradouros.AdicionarAsync(NovoLogradouro());
        await Assert.ThrowsAsync<InvalidOperationException>(() => Logradouros.AdicionarAsync(NovoLogradouro()));
        var segundo = await Logradouros.AdicionarAsync(NovoLogradouro("88010001"));
        segundo.Cep = "88010000";
        await Assert.ThrowsAsync<InvalidOperationException>(() => Logradouros.AtualizarAsync(segundo));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abcdef")]
    public async Task CepInvalido_NaoPersiste(string cep)
    {
        Assert.False(await Logradouros.CepJaExisteAsync(cep));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Logradouros.ObterPorCepAsync(cep));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Logradouros.AdicionarAsync(NovoLogradouro(cep)));
        Assert.Empty(await Logradouros.ObterTodosAsync());
    }

    [Fact]
    public async Task Cancelamento_NaoInsereRegistro()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Logradouros.AdicionarAsync(NovoLogradouro(), cts.Token));
        Assert.Empty(await Logradouros.ObterTodosAsync());
    }

    [Fact]
    public async Task ConsultasInvalidasEResgistroInexistente_SaoTratados()
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Logradouros.ObterPorCidadeAsync(""));
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Logradouros.ObterPorBairroAsync("Cidade", ""));
        var dto = NovoLogradouro();
        dto.Id = 99;
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Logradouros.AtualizarAsync(dto));
    }
}
