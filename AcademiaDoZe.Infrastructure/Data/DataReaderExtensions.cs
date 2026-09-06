// Matheus Marques Stefani
using System.Data.Common;
using System.Globalization;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DataReaderExtensions
{
    public static string GetStringValue(this DbDataReader reader, string column) => Convert.ToString(reader[column], CultureInfo.InvariantCulture)!;
    public static string GetNullableString(this DbDataReader reader, string column) => reader[column] is DBNull ? "" : reader.GetStringValue(column);
    public static int GetInt32Value(this DbDataReader reader, string column) => Convert.ToInt32(reader[column], CultureInfo.InvariantCulture);
    public static byte[]? GetNullableBytes(this DbDataReader reader, string column) => reader[column] is DBNull ? null : (byte[])reader[column];
    public static DateOnly GetDateOnlyValue(this DbDataReader reader, string column) =>
        DateOnly.FromDateTime(Convert.ToDateTime(reader[column], CultureInfo.InvariantCulture));

    public static T ValidarMapeamento<T>(this Result<T> result)
    {
        if (result.IsFailure)
            throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO",
                string.Join(", ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }
}
