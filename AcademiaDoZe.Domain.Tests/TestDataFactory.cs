// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests;

internal static class TestDataFactory
{
    public static Arquivo ArquivoValido() =>
        Arquivo.Criar([1, 2, 3]).Value!;

    public static Logradouro LogradouroValido(int id = 1) =>
        Logradouro.Criar(
            id,
            "12345-678",
            "Rua Teste",
            "Bairro",
            "Cidade",
            "SP",
            "Brasil").Value!;

    public static Aluno AlunoValido(int id = 1, int idade = 20) =>
        Aluno.Criar(
            id,
            "Joao da Silva",
            "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-idade)),
            "(11) 91234-5678",
            "aluno@example.com",
            LogradouroValido(),
            "123",
            string.Empty,
            "Abcdef",
            ArquivoValido()).Value!;

    public static Colaborador ColaboradorValido(
        int id = 1,
        ColaboradorTipo tipo = ColaboradorTipo.Atendente,
        ColaboradorVinculo vinculo = ColaboradorVinculo.CLT) =>
        Colaborador.Criar(
            id,
            "Fulano da Silva",
            "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678",
            "colaborador@example.com",
            LogradouroValido(),
            "123",
            string.Empty,
            "Abcdef",
            ArquivoValido(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            tipo,
            vinculo).Value!;
}
