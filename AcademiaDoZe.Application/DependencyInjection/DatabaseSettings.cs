// Matheus Marques Stefani
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
namespace AcademiaDoZe.Application.DependencyInjection;
public sealed record DatabaseSettings(AppDatabaseType Type, string Path, string Server = "",
    string Database = "", string User = "", string Password = "", string Options = "")
{
    public RepositoryConfig Build()
    {
        var extras = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = Options };
        foreach (string key in extras.Keys)
            if (key.Contains("password", StringComparison.OrdinalIgnoreCase) || key.Equals("pwd", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Use o campo Senha, não as opções complementares, para informar uma senha.");
        string connection;
        if (Type == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(Path) || !System.IO.Path.IsPathFullyQualified(Path))
                throw new ArgumentException("Informe o caminho completo do arquivo SQLite.");
            var builder = new SqliteConnectionStringBuilder(Options)
            { DataSource = System.IO.Path.GetFullPath(Path), ForeignKeys = true, Mode = SqliteOpenMode.ReadWriteCreate };
            connection = builder.ToString();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Server) || string.IsNullOrWhiteSpace(Database) || string.IsNullOrWhiteSpace(User))
                throw new ArgumentException("Preencha servidor, banco e usuário.");
            connection = Type switch
            {
                AppDatabaseType.SqlServer => new SqlConnectionStringBuilder(Options)
                { DataSource = Server, InitialCatalog = Database, UserID = User, Password = Password,
                  IntegratedSecurity = false, ConnectTimeout = 15 }.ToString(),
                AppDatabaseType.MySql => new MySqlConnectionStringBuilder(Options)
                { Server = Server, Database = Database, UserID = User, Password = Password, ConnectionTimeout = 15 }.ToString(),
                _ => throw new ArgumentOutOfRangeException(nameof(Type))
            };
        }
        return new RepositoryConfig { ConnectionString = connection, DatabaseType = Type.ToDatabaseType() };
    }
}
