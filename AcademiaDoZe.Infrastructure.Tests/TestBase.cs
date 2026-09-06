// Matheus Marques Stefani
using System.Security.Cryptography;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase : IDisposable
{
    public const string Nome = "Matheus";
    public const string Sobrenome = "Marques Stefani";
    public const string NomeCompleto = "Matheus Marques Stefani";
    private const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;
    public DatabaseType DatabaseType { get; }
    public string Sigla => DatabaseType switch { DatabaseType.Sqlite => "SQLite", DatabaseType.MySql => "MySQL", _ => "SQLServer" };
    protected string ConnectionString { get; }
    protected LogradouroRepository Logradouros { get; }
    protected AlunoRepository Alunos { get; }
    protected ColaboradorRepository Colaboradores { get; }
    protected MatriculaRepository Matriculas { get; }
    protected static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

    protected TestBase()
    {
        var selected = Environment.GetEnvironmentVariable("ACADEMIA_DB");
        DatabaseType = string.IsNullOrWhiteSpace(selected) ? SelectedDatabaseType : Enum.Parse<DatabaseType>(selected, true);
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(root.FullName, "AcademiaDoZe.sln")))
            root = root.Parent ?? throw new InvalidOperationException("Solução não encontrada.");
        var data = Path.Combine(root.FullName, "Dados");
        Directory.CreateDirectory(data);
        // Mantém os registros dos testes para os SELECTs e prints solicitados nas atividades.
        // Os testes usam identificadores próprios e podem ser executados novamente.
        var defaultSqlite = $"Data Source={Path.Combine(data, "db_academia_do_ze.db")};Foreign Keys=True;Pooling=False";
        ConnectionString = Environment.GetEnvironmentVariable("ACADEMIA_CONNECTION_STRING") ?? (DatabaseType == DatabaseType.Sqlite
            ? defaultSqlite
            : throw new InvalidOperationException("Defina ACADEMIA_CONNECTION_STRING para o banco escolhido. Consulte o README."));
        Logradouros = new(ConnectionString, DatabaseType);
        Alunos = new(ConnectionString, DatabaseType);
        Colaboradores = new(ConnectionString, DatabaseType);
        Matriculas = new(ConnectionString, DatabaseType);
    }

    protected static string GerarCep() => RandomNumberGenerator.GetInt32(10000000, 99999999).ToString();
    protected static string GerarEmail() => $"teste{Guid.NewGuid():N}@example.com";
    protected static string GerarCpf()
    {
        var digits = Enumerable.Range(0, 9).Select(_ => RandomNumberGenerator.GetInt32(10)).ToList();
        for (int count = 9; count <= 10; count++)
        {
            var sum = digits.Select((n, i) => n * (count + 1 - i)).Sum();
            var remainder = sum % 11;
            digits.Add(remainder < 2 ? 0 : 11 - remainder);
        }
        return string.Concat(digits);
    }
    protected Logradouro NovoLogradouro(int id = 0, string? cep = null) =>
        Logradouro.Criar(id, cep ?? GerarCep(), Nome, Sobrenome, Sigla, "SC", "Brasil").ValidarMapeamento();
    protected Task<Logradouro> CriarLogradouro() => Logradouros.Adicionar(NovoLogradouro());
    protected Aluno NovoAluno(Logradouro l, int id = 0, string? cpf = null, string? email = null, string numero = "100") =>
        Aluno.Criar(id, Nome, cpf ?? GerarCpf(), new DateOnly(1995, 5, 15), "49999998888", email ?? GerarEmail(),
            l, numero, Sobrenome, Sigla + "123!", Arquivo.Criar([1, 2, 3, 4]).ValidarMapeamento()).ValidarMapeamento();
    protected Colaborador NovoColaborador(Logradouro l, int id = 0, string? cpf = null, string? email = null,
        ColaboradorTipo tipo = ColaboradorTipo.Instrutor, ColaboradorVinculo vinculo = ColaboradorVinculo.CLT, string numero = "100") =>
        Colaborador.Criar(id, Nome, cpf ?? GerarCpf(), new DateOnly(1995, 5, 15), "49999998888", email ?? GerarEmail(),
            l, numero, Sobrenome, Sigla + "123!", Arquivo.Criar([5, 6, 7, 8]).ValidarMapeamento(),
            new DateOnly(2023, 1, 1), tipo, vinculo).ValidarMapeamento();
    protected async Task<Aluno> CriarAluno() => await Alunos.Adicionar(NovoAluno(await CriarLogradouro()));
    protected async Task<Colaborador> CriarColaborador() => await Colaboradores.Adicionar(NovoColaborador(await CriarLogradouro()));
    protected Matricula NovaMatricula(Aluno aluno, int id = 0, MatriculaPlano plano = MatriculaPlano.Mensal,
        DateOnly? inicio = null, MatriculaRestricoes restricoes = MatriculaRestricoes.None) =>
        Matricula.Criar(id, aluno, plano, inicio ?? Hoje, NomeCompleto, restricoes,
            restricoes == MatriculaRestricoes.None ? null : Arquivo.Criar([10, 20, 30]).ValidarMapeamento(), Sigla).ValidarMapeamento();

    public void Dispose()
    {
        Logradouros.Dispose(); Alunos.Dispose(); Colaboradores.Dispose(); Matriculas.Dispose();
        GC.SuppressFinalize(this);
    }
}
