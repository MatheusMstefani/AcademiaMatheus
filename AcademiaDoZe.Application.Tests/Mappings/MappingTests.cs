// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;

namespace AcademiaDoZe.Application.Tests.Mappings;

public class MappingTests : ApplicationTestBase
{
    [Fact]
    public void Arquivos_NoDtoNaoAlteramConteudoDaEntidade()
    {
        var log = NovoLogradouro();
        log.Id = 1;
        var dto = NovoAluno(log);
        var aluno = dto.ToEntity();
        dto.Foto!.Conteudo[0] = 9;
        Assert.Equal((byte)1, aluno.Foto.Conteudo[0]);
        var saida = aluno.ToDto(log.ToEntity());
        saida.Foto!.Conteudo[0] = 8;
        Assert.Equal((byte)1, aluno.Foto.Conteudo[0]);
        Assert.Null(saida.Senha);
    }

    [Fact]
    public void Enums_ConvertemValoresEDescricoes()
    {
        Assert.Equal("Estagiário", AppColaboradorVinculo.Estagio.GetDisplayName());
        Assert.Equal("Nenhuma Restrição", AppMatriculaRestricoes.None.GetDisplayName());
        Assert.Equal("Diabetes, Pressão Alta",
            (AppMatriculaRestricoes.Diabetes | AppMatriculaRestricoes.PressaoAlta).GetDisplayName());
        foreach (var plano in Enum.GetValues<AppMatriculaPlano>())
            Assert.Equal(plano, plano.ToDomain().ToApplication());
        foreach (var tipo in Enum.GetValues<AppColaboradorTipo>())
            Assert.Equal(tipo, tipo.ToDomain().ToApplication());
    }
}
