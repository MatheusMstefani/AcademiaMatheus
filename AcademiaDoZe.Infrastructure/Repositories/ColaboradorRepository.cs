// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

public sealed class ColaboradorRepository(string connectionString, DatabaseType databaseType)
    : PessoaRepository<Colaborador>(connectionString, databaseType), IColaboradorRepository
{
    protected override string Table => "tb_colaborador";
    protected override string IdColumn => "id_colaborador";
    protected override string ExtraColumns => ",admissao,tipo,vinculo";
    protected override string ExtraValues => ",@Admissao,@Tipo,@Vinculo";
    protected override string ExtraUpdate => ",admissao=@Admissao,tipo=@Tipo,vinculo=@Vinculo";
    protected override Colaborador MapPessoa(DbDataReader reader) => Map(reader);
    protected override void ExtraParameters(DbCommand c, Colaborador e)
    {
        c.AddParameter("@Admissao", e.DataAdmissao, DbType.Date);
        c.AddParameter("@Tipo", (int)e.Tipo, DbType.Int32);
        c.AddParameter("@Vinculo", (int)e.Vinculo, DbType.Int32);
    }
    public static Colaborador Map(DbDataReader r, string nomeColumn = "nome") => Colaborador.Criar(
        r.GetInt32Value("id_colaborador"), r.GetStringValue(nomeColumn), r.GetStringValue("cpf"),
        r.GetDateOnlyValue("nascimento"), r.GetStringValue("telefone"), r.GetStringValue("email"),
        LogradouroRepository.Map(r, "logradouro_nome"), r.GetStringValue("numero"), r.GetNullableString("complemento"),
        r.GetStringValue("senha"), LerFoto(r)!, r.GetDateOnlyValue("admissao"),
        (ColaboradorTipo)r.GetInt32Value("tipo"), (ColaboradorVinculo)r.GetInt32Value("vinculo")).ValidarMapeamento();
    public Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE p.tipo=@Tipo ORDER BY p.nome", MapPessoa, c => c.AddParameter("@Tipo", (int)tipo, DbType.Int32), cancellationToken);
    public Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE p.vinculo=@Vinculo ORDER BY p.nome", MapPessoa, c => c.AddParameter("@Vinculo", (int)vinculo, DbType.Int32), cancellationToken);
}
