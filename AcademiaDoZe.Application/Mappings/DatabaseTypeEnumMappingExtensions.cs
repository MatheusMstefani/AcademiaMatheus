// Matheus Marques Stefani
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Application.Mappings;
public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToDatabaseType(this AppDatabaseType value) => value switch
    {
        AppDatabaseType.SqlServer => DatabaseType.SqlServer, AppDatabaseType.MySql => DatabaseType.MySql,
        AppDatabaseType.Sqlite => DatabaseType.Sqlite, _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
    public static AppDatabaseType ToAppDatabaseType(this DatabaseType value) => value switch
    {
        DatabaseType.SqlServer => AppDatabaseType.SqlServer, DatabaseType.MySql => AppDatabaseType.MySql,
        DatabaseType.Sqlite => AppDatabaseType.Sqlite, _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
}
