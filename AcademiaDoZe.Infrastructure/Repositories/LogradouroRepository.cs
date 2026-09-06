// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

public sealed class LogradouroRepository(string connectionString, DatabaseType databaseType)
    : BaseRepository(connectionString, databaseType), ILogradouroRepository
{
    private const string Select = "SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais FROM tb_logradouro";

    public static Logradouro Map(DbDataReader r, string nomeColumn = "nome") => Logradouro.Criar(
        r.GetInt32Value("id_logradouro"), r.GetStringValue("cep"), r.GetStringValue(nomeColumn),
        r.GetStringValue("bairro"), r.GetStringValue("cidade"), r.GetStringValue("estado"), r.GetStringValue("pais")).ValidarMapeamento();

    private static void Parameters(DbCommand c, Logradouro e)
    {
        Id(c, e.Id);
        c.AddParameter("@Cep", e.Cep.Valor, DbType.String);
        c.AddParameter("@Nome", e.Nome, DbType.String);
        c.AddParameter("@Bairro", e.Bairro, DbType.String);
        c.AddParameter("@Cidade", e.Cidade, DbType.String);
        c.AddParameter("@Estado", e.Estado, DbType.String);
        c.AddParameter("@Pais", e.Pais, DbType.String);
    }

    public async Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default) =>
        (await Consultar(Select + " WHERE id_logradouro=@Id", r => Map(r), c => Id(c, id), cancellationToken)).FirstOrDefault();
    public Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default) =>
        Consultar(Select + " ORDER BY nome", r => Map(r), null, cancellationToken);
    public Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default) =>
        Inserir(entity, "INSERT INTO tb_logradouro (cep,nome,bairro,cidade,estado,pais) VALUES (@Cep,@Nome,@Bairro,@Cidade,@Estado,@Pais)",
            c => Parameters(c, entity), cancellationToken);
    public Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default) =>
        AtualizarRegistro(entity, "UPDATE tb_logradouro SET cep=@Cep,nome=@Nome,bairro=@Bairro,cidade=@Cidade,estado=@Estado,pais=@Pais WHERE id_logradouro=@Id",
            c => Parameters(c, entity), cancellationToken);
    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) =>
        await Alterar("DELETE FROM tb_logradouro WHERE id_logradouro=@Id", c => Id(c, id), cancellationToken) > 0;
    public async Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default) =>
        (await Consultar(Select + " WHERE cep=@Cep", r => Map(r), c => c.AddParameter("@Cep", cep.Valor, DbType.String), cancellationToken)).FirstOrDefault();
    public Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default) =>
        Existe("SELECT COUNT(*) FROM tb_logradouro WHERE cep=@Cep AND (@Id IS NULL OR id_logradouro<>@Id)", c =>
        { c.AddParameter("@Cep", cep.Valor, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);
    public Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE cidade=@Cidade ORDER BY bairro,nome", r => Map(r),
            c => c.AddParameter("@Cidade", cidade, DbType.String), cancellationToken);
    public Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE cidade=@Cidade AND bairro=@Bairro ORDER BY nome", r => Map(r), c =>
        { c.AddParameter("@Cidade", cidade, DbType.String); c.AddParameter("@Bairro", bairro, DbType.String); }, cancellationToken);
}
