// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class AlunoService : IAlunoService
{
    private readonly Func<IAlunoRepository> _repoFactory;
    private readonly Func<ILogradouroRepository>? _logradouroRepoFactory;

    public AlunoService(Func<IAlunoRepository> repoFactory, Func<ILogradouroRepository>? logradouroRepoFactory = null)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        _logradouroRepoFactory = logradouroRepoFactory;
    }

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await MapearAsync(await _repoFactory().ObterPorId(id, cancellationToken), cancellationToken);

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
        await MapearAsync(await _repoFactory().ObterTodos(cancellationToken), cancellationToken);

    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        await MapearAsync(await _repoFactory().ObterPorCpf(
            ServiceValidation.Require(Cpf.Criar(cpf), nameof(cpf)), cancellationToken), cancellationToken);

    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await MapearAsync(await _repoFactory().ObterPorEmail(
            ServiceValidation.Require(Email.Criar(email), nameof(email)), cancellationToken), cancellationToken);

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Cpf.Criar(cpf);
        return result.IsSuccess && await _repoFactory().CpfJaExiste(result.Value!, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Email.Criar(email);
        return result.IsSuccess && await _repoFactory().EmailJaExiste(result.Value!, id, cancellationToken);
    }

    public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default) =>
        _repoFactory().Remover(id, cancellationToken);

    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        return await MapearAsync(await _repoFactory().ObterPorNome(nome.Trim(), cancellationToken), cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hash = ServiceValidation.HashPassword(novaSenha);
        var senha = ServiceValidation.Require(Senha.Criar(hash), nameof(novaSenha));
        return await _repoFactory().TrocarSenha(id, senha, cancellationToken);
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        cancellationToken.ThrowIfCancellationRequested();
        if (dto.Id != 0) throw new ArgumentException("Novo cadastro deve ter ID zero.", nameof(dto));
        var repo = _repoFactory();
        await ValidarUnicidadeAsync(repo, dto, null, cancellationToken);
        var logradouro = await BuscarLogradouroAsync(dto.Endereco?.Id ?? 0, dto.Endereco, cancellationToken);
        // Primeiro valida todo o domínio, inclusive foto e datas, depois calcula o hash.
        dto.ToEntity(logradouro);
        var hash = ServiceValidation.HashPassword(dto.Senha);
        var entity = dto.ToEntity(logradouro, hash);
        return (await repo.Adicionar(entity, cancellationToken)).ToDto(logradouro);
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var repo = _repoFactory();
        var atual = await repo.ObterPorId(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Aluno não encontrado.");
        var cpf = ServiceValidation.Require(Cpf.Criar(dto.Cpf), nameof(dto));
        if (cpf.Valor != atual.Cpf.Valor)
            throw new InvalidOperationException("O CPF não pode ser alterado.");
        await ValidarUnicidadeAsync(repo, dto, atual.Id, cancellationToken);
        int logradouroId = dto.Endereco?.Id ?? atual.Endereco.LogradouroId;
        var logradouro = await BuscarLogradouroAsync(logradouroId, dto.Endereco, cancellationToken);
        atual.UpdateFromDto(dto, logradouro);
        var hash = string.IsNullOrWhiteSpace(dto.Senha) ? null : ServiceValidation.HashPassword(dto.Senha);
        var entity = atual.UpdateFromDto(dto, logradouro, hash);
        return (await repo.Atualizar(entity, cancellationToken)).ToDto(logradouro);
    }

    private static async Task ValidarUnicidadeAsync(IAlunoRepository repo, AlunoDto dto,
        int? id, CancellationToken ct)
    {
        var cpf = ServiceValidation.Require(Cpf.Criar(dto.Cpf), nameof(dto));
        if (await repo.CpfJaExiste(cpf, id, ct))
            throw new InvalidOperationException("CPF já cadastrado.");
        if (dto.Email is not null)
        {
            var email = ServiceValidation.Require(Email.Criar(dto.Email), nameof(dto));
            if (await repo.EmailJaExiste(email, id, ct))
                throw new InvalidOperationException("Email já cadastrado.");
        }
    }

    private async Task<Logradouro> BuscarLogradouroAsync(int id, LogradouroDto? dto, CancellationToken ct)
    {
        if (_logradouroRepoFactory is not null)
            return await _logradouroRepoFactory().ObterPorId(id, ct)
                ?? throw new KeyNotFoundException("Logradouro não encontrado.");
        return dto?.ToEntity() ?? throw new InvalidOperationException("Informe o logradouro.");
    }

    private async Task<AlunoDto?> MapearAsync(Aluno? entity, CancellationToken ct)
    {
        if (entity is null) return null;
        var logradouro = _logradouroRepoFactory is null ? null :
            await _logradouroRepoFactory().ObterPorId(entity.Endereco.LogradouroId, ct);
        return entity.ToDto(logradouro);
    }

    private async Task<IEnumerable<AlunoDto>> MapearAsync(IEnumerable<Aluno> entities, CancellationToken ct)
    {
        var lista = entities.ToList();
        var logradouros = new Dictionary<int, Logradouro?>();
        if (_logradouroRepoFactory is not null)
        {
            var repo = _logradouroRepoFactory();
            foreach (int id in lista.Select(e => e.Endereco.LogradouroId).Distinct())
                logradouros[id] = await repo.ObterPorId(id, ct);
        }
        return lista.Select(e => e.ToDto(logradouros.GetValueOrDefault(e.Endereco.LogradouroId))).ToArray();
    }
}
