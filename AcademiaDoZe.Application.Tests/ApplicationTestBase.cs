// Matheus Marques Stefani
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AcademiaDoZe.Application.Tests;

public abstract class ApplicationTestBase : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"AcademiaDoZe-dev08-{Guid.NewGuid():N}.db");
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    protected IServiceProvider Services => _scope.ServiceProvider;
    protected ILogradouroService Logradouros => Services.GetRequiredService<ILogradouroService>();
    protected IAlunoService Alunos => Services.GetRequiredService<IAlunoService>();
    protected IColaboradorService Colaboradores => Services.GetRequiredService<IColaboradorService>();
    protected IMatriculaService Matriculas => Services.GetRequiredService<IMatriculaService>();
    protected static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

    protected ApplicationTestBase()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = $"Data Source={_databasePath};Foreign Keys=True;Pooling=False",
            DatabaseType = DatabaseType.Sqlite
        });
        services.AddApplicationServices();
        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        _scope = _provider.CreateScope();
    }

    protected static LogradouroDto NovoLogradouro(string cep = "88010000") => new()
    {
        Cep = cep, Nome = " Rua de Teste ", Bairro = "Centro",
        Cidade = "Florianópolis", Estado = "sc", Pais = "Brasil"
    };

    protected static AlunoDto NovoAluno(LogradouroDto logradouro) => new()
    {
        Nome = "Matheus Marques Stefani", Cpf = "52998224725",
        DataNascimento = new DateOnly(1995, 5, 15), Telefone = "49999998888",
        Email = "matheus@example.com", Endereco = logradouro, Numero = "100",
        Complemento = "Casa", Senha = "MinhaSenha123!", Foto = new ArquivoDto { Conteudo = [1, 2, 3] }
    };

    protected static ColaboradorDto NovoColaborador(LogradouroDto logradouro) => new()
    {
        Nome = "Matheus Marques Stefani", Cpf = "52998224725",
        DataNascimento = new DateOnly(1995, 5, 15), Telefone = "49999998888",
        Email = "colaborador@example.com", Endereco = logradouro, Numero = "100",
        Complemento = "Casa", Senha = "MinhaSenha123!", Foto = new ArquivoDto { Conteudo = [1, 2, 3] },
        DataAdmissao = new DateOnly(2023, 1, 1), Tipo = AppColaboradorTipo.Instrutor,
        Vinculo = AppColaboradorVinculo.Estagio
    };

    protected async Task<AlunoDto> CriarAluno() => await Alunos.AdicionarAsync(NovoAluno(await Logradouros.AdicionarAsync(NovoLogradouro())));

    protected static MatriculaDto NovaMatricula(AlunoDto aluno) => new()
    {
        AlunoMatricula = aluno, Plano = AppMatriculaPlano.Mensal, DataInicio = Hoje,
        Objetivo = "Matheus Marques Stefani", RestricoesMedicas = AppMatriculaRestricoes.None,
        ObservacoesRestricoes = "SQLite"
    };

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        // Arquivo temporário exclusivo deste teste; não altera o banco usado nos prints.
        if (File.Exists(_databasePath)) File.Delete(_databasePath);
        GC.SuppressFinalize(this);
    }
}
