// Matheus Marques Stefani
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Domain.Tests.Repositories;

public class RepositoryTests
{
    [Theory(DisplayName = "Repositórios específicos herdam do contrato genérico")]
    [InlineData(typeof(ILogradouroRepository), typeof(IRepository<Logradouro>))]
    [InlineData(typeof(IAlunoRepository), typeof(IRepository<Aluno>))]
    [InlineData(typeof(IColaboradorRepository), typeof(IRepository<Colaborador>))]
    [InlineData(typeof(IMatriculaRepository), typeof(IRepository<Matricula>))]
    [InlineData(typeof(IAcessoAlunoRepository), typeof(IRepository<AcessoAluno>))]
    [InlineData(typeof(IAcessoColaboradorRepository), typeof(IRepository<AcessoColaborador>))]
    public void RepositorioEspecifico_DeveHerdarContratoGenerico(
        Type repositorio,
        Type contrato)
    {
        Assert.True(contrato.IsAssignableFrom(repositorio));
    }

    [Theory(DisplayName = "Contrato genérico possui operações básicas assíncronas")]
    [InlineData("ObterPorId")]
    [InlineData("ObterTodos")]
    [InlineData("Adicionar")]
    [InlineData("Atualizar")]
    [InlineData("Remover")]
    public void RepositorioGenerico_DevePossuirOperacao(string nome)
    {
        var metodo = typeof(IRepository<>).GetMethod(nome);

        Assert.NotNull(metodo);
        Assert.True(typeof(Task).IsAssignableFrom(metodo!.ReturnType));
    }

    [Fact(DisplayName = "Contrato genérico é restrito a raízes de agregado")]
    public void RepositorioGenerico_DevePossuirRestricoes()
    {
        var parametro = typeof(IRepository<>).GetGenericArguments().Single();
        var restricoes = parametro.GetGenericParameterConstraints();

        Assert.Contains(typeof(Entity), restricoes);
        Assert.Contains(typeof(AcademiaDoZe.Domain.Common.IAggregateRoot), restricoes);
    }
}
