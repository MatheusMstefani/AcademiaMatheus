// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

public sealed class AlunoRepository(string connectionString, DatabaseType databaseType)
    : PessoaRepository<Aluno>(connectionString, databaseType), IAlunoRepository
{
    protected override string Table => "tb_aluno";
    protected override string IdColumn => "id_aluno";
    protected override Aluno MapPessoa(DbDataReader reader) => Map(reader);

    public static Aluno Map(DbDataReader r, string nomeColumn = "nome") => Aluno.Criar(
        r.GetInt32Value("id_aluno"), r.GetStringValue(nomeColumn), r.GetStringValue("cpf"),
        r.GetDateOnlyValue("nascimento"), r.GetStringValue("telefone"), r.GetStringValue("email"),
        LogradouroRepository.Map(r, "logradouro_nome"), r.GetStringValue("numero"), r.GetNullableString("complemento"),
        r.GetStringValue("senha"), LerFoto(r)!).ValidarMapeamento();

    public Task<IEnumerable<Aluno>> ObterPorNome(string nome, CancellationToken cancellationToken = default)
    {
        // Escape dos curingas: pesquisar um nome não deve interpretar % ou _ como filtro.
        var literal = nome.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_").Replace("[", "![");
        return Consultar(Select + " WHERE p.nome LIKE @Nome ESCAPE '!' ORDER BY p.nome", MapPessoa,
            c => c.AddParameter("@Nome", "%" + literal + "%", DbType.String), cancellationToken);
    }
}
