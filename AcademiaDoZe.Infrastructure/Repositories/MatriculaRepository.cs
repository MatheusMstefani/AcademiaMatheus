// Matheus Marques Stefani
using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;

namespace AcademiaDoZe.Infrastructure.Repositories;

public sealed class MatriculaRepository(string connectionString, DatabaseType databaseType)
    : BaseRepository(connectionString, databaseType), IMatriculaRepository
{
    private const string Select = """
        SELECT m.*,a.id_aluno,a.cpf,a.nome AS aluno_nome,a.nascimento,a.telefone,a.email,a.numero,a.complemento,a.senha,a.foto,
               l.id_logradouro,l.cep,l.nome AS logradouro_nome,l.bairro,l.cidade,l.estado,l.pais
        FROM tb_matricula m INNER JOIN tb_aluno a ON m.aluno_id=a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id=l.id_logradouro
        """;

    public static Matricula Map(DbDataReader r)
    {
        var bytes = r.GetNullableBytes("laudo_medico");
        var entity = Matricula.Criar(r.GetInt32Value("id_matricula"), AlunoRepository.Map(r, "aluno_nome"),
            (MatriculaPlano)r.GetInt32Value("plano"), r.GetDateOnlyValue("data_inicio"), r.GetStringValue("objetivo"),
            (MatriculaRestricoes)r.GetInt32Value("restricao_medica"),
            bytes is null ? null : Arquivo.Criar(bytes).ValidarMapeamento(), r.GetNullableString("obs_restricao")).ValidarMapeamento();
        if (entity.DataFim != r.GetDateOnlyValue("data_fim"))
            throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", "A data final salva não corresponde ao plano da matrícula.");
        return entity;
    }

    private static void Parameters(DbCommand c, Matricula e)
    {
        Id(c, e.Id);
        c.AddParameter("@AlunoId", e.AlunoId, DbType.Int32);
        c.AddParameter("@Plano", (int)e.Plano, DbType.Int32);
        c.AddParameter("@Inicio", e.DataInicio, DbType.Date);
        c.AddParameter("@Fim", e.DataFim, DbType.Date);
        c.AddParameter("@Objetivo", e.Objetivo, DbType.String);
        c.AddParameter("@Restricao", (int)e.RestricoesMedicas, DbType.Int32);
        c.AddParameter("@Obs", e.ObservacoesRestricoes, DbType.String);
        c.AddParameter("@Laudo", e.LaudoMedico?.Conteudo, DbType.Binary);
    }
    public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default) =>
        (await Consultar(Select + " WHERE m.id_matricula=@Id", Map, c => Id(c, id), cancellationToken)).FirstOrDefault();
    public Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default) =>
        Consultar(Select + " ORDER BY m.data_inicio DESC", Map, null, cancellationToken);
    public Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default) =>
        Inserir(entity, "INSERT INTO tb_matricula (aluno_id,plano,data_inicio,data_fim,objetivo,restricao_medica,obs_restricao,laudo_medico) VALUES (@AlunoId,@Plano,@Inicio,@Fim,@Objetivo,@Restricao,@Obs,@Laudo)", c => Parameters(c, entity), cancellationToken);
    public Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default) =>
        AtualizarRegistro(entity, "UPDATE tb_matricula SET aluno_id=@AlunoId,plano=@Plano,data_inicio=@Inicio,data_fim=@Fim,objetivo=@Objetivo,restricao_medica=@Restricao,obs_restricao=@Obs,laudo_medico=@Laudo WHERE id_matricula=@Id", c => Parameters(c, entity), cancellationToken);
    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default) =>
        await Alterar("DELETE FROM tb_matricula WHERE id_matricula=@Id", c => Id(c, id), cancellationToken) > 0;
    public Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE m.aluno_id=@Id ORDER BY m.data_inicio DESC", Map, c => Id(c, alunoId), cancellationToken);

    // Os parâmetros de data usam o mesmo dia local nos três bancos, sem diferenças de fuso ou hora.
    public Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE m.data_inicio<=@Hoje AND m.data_fim>=@Hoje AND (@Id=0 OR m.aluno_id=@Id) ORDER BY m.data_fim DESC", Map, c =>
        { Id(c, alunoId); c.AddParameter("@Hoje", DateOnly.FromDateTime(DateTime.Today), DbType.Date); }, cancellationToken);
    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default) =>
        alunoId <= 0 ? null : (await ObterAtivas(alunoId, cancellationToken)).FirstOrDefault();
    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default) =>
        await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken) is not null;
    public Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(dias);
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return Consultar(Select + " WHERE m.data_inicio<=@Hoje AND m.data_fim>=@Hoje AND m.data_fim<=@Limite ORDER BY m.data_fim", Map, c =>
        { c.AddParameter("@Hoje", hoje, DbType.Date); c.AddParameter("@Limite", hoje.AddDays(dias), DbType.Date); }, cancellationToken);
    }
    public Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default) =>
        Consultar(Select + " WHERE m.plano=@Plano ORDER BY a.nome", Map, c => c.AddParameter("@Plano", (int)plano, DbType.Int32), cancellationToken);
}
