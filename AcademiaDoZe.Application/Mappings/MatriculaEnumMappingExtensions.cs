// Matheus Marques Stefani
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaEnumMappingExtensions
{
    public static MatriculaPlano ToDomain(this AppMatriculaPlano value) => (MatriculaPlano)value;
    public static AppMatriculaPlano ToApplication(this MatriculaPlano value) => (AppMatriculaPlano)value;
    public static MatriculaRestricoes ToDomain(this AppMatriculaRestricoes value) => (MatriculaRestricoes)value;
    public static AppMatriculaRestricoes ToApplication(this MatriculaRestricoes value) => (AppMatriculaRestricoes)value;
}
