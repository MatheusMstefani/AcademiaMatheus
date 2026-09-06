// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

// Aluno e colaborador compartilham os dados pessoais, mas mantêm seus contratos próprios.
public abstract class PessoaRepository<T>(string connectionString, DatabaseType databaseType)
    : BaseRepository(connectionString, databaseType) where T : Pessoa
{
    protected abstract string Table { get; }
    protected abstract string IdColumn { get; }
    protected abstract T MapPessoa(DbDataReader reader);
    protected virtual string ExtraColumns => "";
    protected virtual string ExtraValues => "";
    protected virtual string ExtraUpdate => "";
    protected virtual void ExtraParameters(DbCommand c, T e) { }
    protected string Select => $"SELECT p.*,l.id_logradouro,l.cep,l.nome AS logradouro_nome,l.bairro,l.cidade,l.estado,l.pais FROM {Table} p INNER JOIN tb_logradouro l ON p.logradouro_id=l.id_logradouro";

    private void Parameters(DbCommand c, T e)
    {
        Id(c, e.Id);
        c.AddParameter("@Cpf", e.Cpf.Valor, DbType.String);
        c.AddParameter("@Nome", e.Nome, DbType.String);
        c.AddParameter("@Nascimento", e.DataNascimento, DbType.Date);
        c.AddParameter("@Telefone", e.Telefone.Valor, DbType.String);
        c.AddParameter("@Email", e.Email.Valor, DbType.String);
        c.AddParameter("@LogradouroId", e.Endereco.LogradouroId, DbType.Int32);
        c.AddParameter("@Numero", e.Endereco.Numero, DbType.String);
        c.AddParameter("@Complemento", e.Endereco.Complemento, DbType.String);
        c.AddParameter("@Senha", e.Senha.Valor, DbType.String);
        c.AddParameter("@Foto", e.Foto?.Conteudo, DbType.Binary);
        ExtraParameters(c, e);
    }

    public async Task<T?> ObterPorId(int id, CancellationToken cancellationToken = default) =>
        (await Consultar(Select + $" WHERE p.{IdColumn}=@Id", MapPessoa, c => Id(c, id), cancellationToken)).FirstOrDefault();
    public Task<IEnumerable<T>> ObterTodos(CancellationToken cancellationToken = default) =>
        Consultar(Select + " ORDER BY p.nome", MapPessoa, null, cancellationToken);
    public Task<T> Adicionar(T entity, CancellationToken cancellationToken = default) =>
        Inserir(entity, $"INSERT INTO {Table} (cpf,nome,nascimento,telefone,email,logradouro_id,numero,complemento,senha,foto{ExtraColumns}) VALUES (@Cpf,@Nome,@Nascimento,@Telefone,@Email,@LogradouroId,@Numero,@Complemento,@Senha,@Foto{ExtraValues})", c => Parameters(c, entity), cancellationToken);
    public Task<T> Atualizar(T entity, CancellationToken cancellationToken = default) =>
        AtualizarRegistro(entity, $"UPDATE {Table} SET cpf=@Cpf,nome=@Nome,nascimento=@Nascimento,telefone=@Telefone,email=@Email,logradouro_id=@LogradouroId,numero=@Numero,complemento=@Complemento,senha=@Senha,foto=@Foto{ExtraUpdate} WHERE {IdColumn}=@Id", c => Parameters(c, entity), cancellationToken);
    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) =>
        await Alterar($"DELETE FROM {Table} WHERE {IdColumn}=@Id", c => Id(c, id), cancellationToken) > 0;
    public async Task<T?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default) =>
        (await Consultar(Select + " WHERE p.cpf=@Cpf", MapPessoa, c => c.AddParameter("@Cpf", cpf.Valor, DbType.String), cancellationToken)).FirstOrDefault();
    public async Task<T?> ObterPorEmail(Email email, CancellationToken cancellationToken = default) =>
        (await Consultar(Select + " WHERE p.email=@Email", MapPessoa, c => c.AddParameter("@Email", email.Valor, DbType.String), cancellationToken)).FirstOrDefault();
    public Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default) =>
        Existe($"SELECT COUNT(*) FROM {Table} WHERE cpf=@Cpf AND (@Id IS NULL OR {IdColumn}<>@Id)", c =>
        { c.AddParameter("@Cpf", cpf.Valor, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);
    public Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default) =>
        Existe($"SELECT COUNT(*) FROM {Table} WHERE email=@Email AND (@Id IS NULL OR {IdColumn}<>@Id)", c =>
        { c.AddParameter("@Email", email.Valor, DbType.String); c.AddParameter("@Id", id, DbType.Int32); }, cancellationToken);
    public async Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default) =>
        await Alterar($"UPDATE {Table} SET senha=@Senha WHERE {IdColumn}=@Id", c =>
        { Id(c, id); c.AddParameter("@Senha", novaSenha.Valor, DbType.String); }, cancellationToken) > 0;

    protected static Arquivo? LerFoto(DbDataReader r)
    {
        var bytes = r.GetNullableBytes("foto");
        return bytes is null ? null : Arquivo.Criar(bytes).ValidarMapeamento();
    }
}
