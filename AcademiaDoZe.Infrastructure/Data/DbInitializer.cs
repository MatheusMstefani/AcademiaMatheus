// Matheus Marques Stefani
using System.Data.Common;
using System.Collections.Concurrent;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    private static readonly ConcurrentDictionary<string, bool> Inicializados = new();

    public static string ObterScript(DatabaseType type)
    {
        var assembly = typeof(DbInitializer).Assembly;
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith(DbProvider.GetScriptName(type)));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static async Task InicializarAsync(string connectionString, DatabaseType type, CancellationToken cancellationToken = default)
    {
        var key = $"{type}:{connectionString}";
        var gate = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (Inicializados.ContainsKey(key)) return;
            await using var connection = DbProvider.CreateConnection(connectionString, type);
            await connection.OpenAsync(cancellationToken);
            await using var command = DbProvider.CreateCommand(ObterScript(type), connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            Inicializados[key] = true;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_INICIALIZAR_BANCO", "Não foi possível preparar as tabelas.", ex);
        }
        finally { gate.Release(); }
    }
}
