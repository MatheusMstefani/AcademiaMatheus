// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;

    public MatriculaService(Func<IMatriculaRepository> matriculaRepoFactory, Func<IAlunoRepository> alunoRepoFactory)
    {
        _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _matriculaRepoFactory().ObterPorId(id, cancellationToken);
        return entity is null ? null : entity.ToDto((await BuscarAlunoAsync(entity.AlunoId, cancellationToken)).ToDto());
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default) =>
        await MapearAsync(await _matriculaRepoFactory().ObterTodos(cancellationToken), cancellationToken);

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var aluno = await BuscarAlunoAsync(alunoId, cancellationToken);
        return (await _matriculaRepoFactory().ObterPorAluno(alunoId, cancellationToken))
            .Select(e => e.ToDto(aluno.ToDto())).ToArray();
    }

    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var entity = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return entity is null ? null : entity.ToDto((await BuscarAlunoAsync(alunoId, cancellationToken)).ToDto());
    }

    public Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default) =>
        _matriculaRepoFactory().PossuiMatriculaAtiva(alunoId, cancellationToken);

    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default) =>
        await MapearAsync(await _matriculaRepoFactory().ObterAtivas(alunoId, cancellationToken), cancellationToken);

    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        if (dias < 0) throw new ArgumentOutOfRangeException(nameof(dias));
        return await MapearAsync(await _matriculaRepoFactory().ObterVencendoEmDias(dias, cancellationToken), cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(plano)) throw new ArgumentOutOfRangeException(nameof(plano));
        return await MapearAsync(await _matriculaRepoFactory().ObterPorPlano(plano.ToDomain(), cancellationToken), cancellationToken);
    }

    public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default) =>
        _matriculaRepoFactory().Remover(id, cancellationToken);

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Id != 0) throw new ArgumentException("Nova matrícula deve ter ID zero.", nameof(dto));
        if (dto.AlunoMatricula is null || dto.AlunoMatricula.Id <= 0)
            throw new InvalidOperationException("Informe um aluno cadastrado.");
        // Usa o aluno persistido, sem reconstruir uma pessoa a partir de um DTO sem senha.
        var aluno = await BuscarAlunoAsync(dto.AlunoMatricula.Id, cancellationToken);
        var repo = _matriculaRepoFactory();
        if (await repo.PossuiMatriculaAtiva(aluno.Id, cancellationToken))
            throw new InvalidOperationException("Já existe uma matrícula ativa para este aluno.");
        ValidarLaudo(dto, aluno, dto.LaudoMedico?.Conteudo is { Length: > 0 });
        var entity = dto.ToEntity(aluno);
        return (await repo.Adicionar(entity, cancellationToken)).ToDto(aluno.ToDto());
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var repo = _matriculaRepoFactory();
        var atual = await repo.ObterPorId(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Matrícula não encontrada.");
        if (dto.AlunoMatricula is null || dto.AlunoMatricula.Id != atual.AlunoId)
            throw new InvalidOperationException("Não é permitido trocar o aluno da matrícula.");
        var aluno = await BuscarAlunoAsync(atual.AlunoId, cancellationToken);
        bool possuiLaudo = dto.LaudoMedico is null ? atual.LaudoMedico is not null :
            dto.LaudoMedico.Conteudo is { Length: > 0 };
        ValidarLaudo(dto, aluno, possuiLaudo);
        var entity = atual.UpdateFromDto(dto, aluno);
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        if (entity.DataInicio <= hoje && entity.DataFim >= hoje)
        {
            var ativas = await repo.ObterAtivas(aluno.Id, cancellationToken);
            if (ativas.Any(m => m.Id != atual.Id))
                throw new InvalidOperationException("Já existe outra matrícula ativa para este aluno.");
        }
        return (await repo.Atualizar(entity, cancellationToken)).ToDto(aluno.ToDto());
    }

    private async Task<Aluno> BuscarAlunoAsync(int id, CancellationToken ct) =>
        await _alunoRepoFactory().ObterPorId(id, ct)
            ?? throw new InvalidOperationException("Aluno não encontrado.");

    private static void ValidarLaudo(MatriculaDto dto, Aluno aluno, bool possuiLaudo)
    {
        bool menorDe16 = aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        if (menorDe16 && !possuiLaudo)
            throw new InvalidOperationException("Alunos menores de 16 anos precisam de laudo médico.");
        if (dto.RestricoesMedicas != AppMatriculaRestricoes.None && !possuiLaudo)
            throw new InvalidOperationException("Alunos com restrições de saúde precisam de laudo médico.");
    }

    private async Task<IEnumerable<MatriculaDto>> MapearAsync(IEnumerable<Matricula> entities, CancellationToken ct)
    {
        var lista = entities.ToList();
        var alunos = new Dictionary<int, AlunoDto>();
        foreach (int id in lista.Select(m => m.AlunoId).Distinct())
            alunos[id] = (await BuscarAlunoAsync(id, ct)).ToDto();
        return lista.Select(m => m.ToDto(alunos[m.AlunoId])).ToArray();
    }
}
