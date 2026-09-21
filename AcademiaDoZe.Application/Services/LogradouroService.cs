// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class LogradouroService : ILogradouroService
{
    private readonly Func<ILogradouroRepository> _repoFactory;
    public LogradouroService(Func<ILogradouroRepository> repoFactory) =>
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));

    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        (await _repoFactory().ObterPorId(id, cancellationToken))?.ToDto();

    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
        (await _repoFactory().ObterTodos(cancellationToken)).Select(e => e.ToDto()).ToArray();

    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default) =>
        (await _repoFactory().ObterPorCep(ServiceValidation.Require(Cep.Criar(cep), nameof(cep)), cancellationToken))?.ToDto();

    public async Task<bool> CepJaExisteAsync(string cep, int? id = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Cep.Criar(cep);
        return result.IsSuccess && await _repoFactory().CepJaExiste(result.Value!, id, cancellationToken);
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cidade);
        return (await _repoFactory().ObterPorCidade(cidade.Trim(), cancellationToken)).Select(e => e.ToDto()).ToArray();
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cidade);
        ArgumentException.ThrowIfNullOrWhiteSpace(bairro);
        return (await _repoFactory().ObterPorBairro(cidade.Trim(), bairro.Trim(), cancellationToken)).Select(e => e.ToDto()).ToArray();
    }

    public async Task<LogradouroDto> AdicionarAsync(LogradouroDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Id != 0) throw new ArgumentException("Novo cadastro deve ter ID zero.", nameof(dto));
        var cep = ServiceValidation.Require(Cep.Criar(dto.Cep), nameof(dto));
        var repo = _repoFactory();
        if (await repo.CepJaExiste(cep, null, cancellationToken))
            throw new InvalidOperationException("Já existe um logradouro com esse CEP.");
        return (await repo.Adicionar(dto.ToEntity(), cancellationToken)).ToDto();
    }

    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var repo = _repoFactory();
        var atual = await repo.ObterPorId(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Logradouro não encontrado.");
        var cep = ServiceValidation.Require(Cep.Criar(dto.Cep), nameof(dto));
        if (await repo.CepJaExiste(cep, dto.Id, cancellationToken))
            throw new InvalidOperationException("Já existe outro logradouro com esse CEP.");
        return (await repo.Atualizar(atual.UpdateFromDto(dto), cancellationToken)).ToDto();
    }

    public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default) =>
        _repoFactory().Remover(id, cancellationToken);
}
