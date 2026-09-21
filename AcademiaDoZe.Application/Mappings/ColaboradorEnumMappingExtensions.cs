// Matheus Marques Stefani
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorEnumMappingExtensions
{
    public static ColaboradorTipo ToDomain(this AppColaboradorTipo value) => (ColaboradorTipo)value;
    public static AppColaboradorTipo ToApplication(this ColaboradorTipo value) => (AppColaboradorTipo)value;
    public static ColaboradorVinculo ToDomain(this AppColaboradorVinculo value) => (ColaboradorVinculo)value;
    public static AppColaboradorVinculo ToApplication(this ColaboradorVinculo value) => (AppColaboradorVinculo)value;
}
