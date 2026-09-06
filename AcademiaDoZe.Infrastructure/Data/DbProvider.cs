// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Infrastructure.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;

namespace AcademiaDoZe.Infrastructure.Data;

public enum DatabaseType { SqlServer, MySql, Sqlite }

public static class DbProvider
{
    public static DbConnection CreateConnection(string connectionString, DatabaseType databaseType)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InfrastructureException("CONEXAO_STRING_VAZIA", "Informe a string de conexão.");
        DbProviderFactory factory = databaseType switch
        {
            DatabaseType.SqlServer => SqlClientFactory.Instance,
            DatabaseType.MySql => MySqlClientFactory.Instance,
            DatabaseType.Sqlite => SqliteFactory.Instance,
            _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", "Banco de dados não suportado.")
        };
        var connection = factory.CreateConnection()!;
        // SQLite precisa habilitar as chaves estrangeiras em cada conexão.
        connection.ConnectionString = databaseType == DatabaseType.Sqlite
            ? new SqliteConnectionStringBuilder(connectionString) { ForeignKeys = true }.ToString()
            : connectionString;
        return connection;
    }

    public static DbCommand CreateCommand(string sql, DbConnection connection)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new InfrastructureException("COMANDO_VAZIO", "O comando SQL está vazio.");
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 30;
        return command;
    }

    public static DbParameter AddParameter(this DbCommand command, string name, object? value, DbType type)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        // DateTime funciona nos três provedores, inclusive os que não aceitam DateOnly diretamente.
        parameter.Value = value is DateOnly date ? date.ToDateTime(TimeOnly.MinValue) : value ?? DBNull.Value;
        command.Parameters.Add(parameter);
        return parameter;
    }

    public static string GetScriptName(DatabaseType type) => type switch
    {
        DatabaseType.SqlServer => "script_sqlserver.sql",
        DatabaseType.MySql => "script_mysql.sql",
        DatabaseType.Sqlite => "script_sqlite.sql",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", "Banco de dados não suportado.")
    };

    public static string FormatInsertQuery(string sql, DatabaseType type) => sql + (type switch
    {
        DatabaseType.SqlServer => "; SELECT CAST(SCOPE_IDENTITY() AS int);",
        DatabaseType.MySql => "; SELECT LAST_INSERT_ID();",
        DatabaseType.Sqlite => "; SELECT last_insert_rowid();",
        _ => throw new InfrastructureException("SGBD_NAO_SUPORTADO", "Banco de dados não suportado.")
    });
}
