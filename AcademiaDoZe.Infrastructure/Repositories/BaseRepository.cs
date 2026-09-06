// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class BaseRepository : IDisposable, IAsyncDisposable
{
    protected readonly string ConnectionString;
    protected readonly DatabaseType DatabaseType;
    private bool _disposed;

    protected BaseRepository(string connectionString, DatabaseType databaseType)
    {
        using var validation = DbProvider.CreateConnection(connectionString, databaseType);
        ConnectionString = connectionString;
        DatabaseType = databaseType;
    }

    // Uma conexão por operação permite reutilizar o repositório sem leitores concorrentes.
    protected async Task<T> Executar<T>(string sql, Action<DbCommand>? parameters,
        Func<DbCommand, CancellationToken, Task<T>> action, string errorCode, CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ct.ThrowIfCancellationRequested();
        try
        {
            await DbInitializer.InicializarAsync(ConnectionString, DatabaseType, ct);
            await using var connection = DbProvider.CreateConnection(ConnectionString, DatabaseType);
            await connection.OpenAsync(ct);
            await using var command = DbProvider.CreateCommand(sql, connection);
            parameters?.Invoke(command);
            return await action(command, ct);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(errorCode, "A operação no banco de dados falhou.", ex);
        }
    }

    protected Task<IEnumerable<T>> Consultar<T>(string sql, Func<DbDataReader, T> map,
        Action<DbCommand>? parameters, CancellationToken ct) =>
        Executar<IEnumerable<T>>(sql, parameters, async (command, token) =>
        {
            var items = new List<T>();
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                try { items.Add(map(reader)); }
                catch (Exception ex) when (ex is not InfrastructureException and not OperationCanceledException)
                { throw new InfrastructureException("ERRO_MAPEAMENTO", "Falha ao reconstruir o registro.", ex); }
            }
            return items;
        }, "ERRO_CONSULTAR", ct);

    protected Task<int> Alterar(string sql, Action<DbCommand> parameters, CancellationToken ct) =>
        Executar(sql, parameters, (c, t) => c.ExecuteNonQueryAsync(t), "ERRO_PERSISTENCIA", ct);

    protected Task<bool> Existe(string sql, Action<DbCommand> parameters, CancellationToken ct) =>
        Executar(sql, parameters, async (c, t) => Convert.ToInt64(await c.ExecuteScalarAsync(t)) > 0, "ERRO_VERIFICAR_EXISTENCIA", ct);

    protected async Task<T> Inserir<T>(T entity, string sql, Action<DbCommand> parameters, CancellationToken ct) where T : Entity
    {
        var id = await Executar(DbProvider.FormatInsertQuery(sql, DatabaseType), parameters,
            async (c, t) => Convert.ToInt32(await c.ExecuteScalarAsync(t)), "ERRO_ADICIONAR", ct);
        if (id <= 0) throw new InfrastructureException("ERRO_OBTER_ID", "O banco não retornou um ID válido.");
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        return entity;
    }

    protected async Task<T> AtualizarRegistro<T>(T entity, string sql, Action<DbCommand> parameters, CancellationToken ct)
    {
        if (await Alterar(sql, parameters, ct) == 0)
            throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", "Registro não encontrado para atualização.");
        return entity;
    }

    protected static void Id(DbCommand command, int id) => command.AddParameter("@Id", id, DbType.Int32);
    public void Dispose() { _disposed = true; GC.SuppressFinalize(this); }
    public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
}
