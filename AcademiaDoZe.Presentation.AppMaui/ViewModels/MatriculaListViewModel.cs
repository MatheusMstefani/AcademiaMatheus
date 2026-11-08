// Matheus Marques Stefani
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Validation;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class MatriculaListViewModel : BaseViewModel
{
    private readonly AppServices services;
    private readonly ConfigurationHelper config;
    private int loadedRevision;
    public ObservableCollection<MatriculaDto> Matriculas { get; } = [];
    public string[] Campos { get; } = ["Todas", "Id", "Id Aluno", "CPF", "Ativas", "Vencendo em dias"];
    private string campo = "Todas";
    public string Campo { get => campo; set => SetProperty(ref campo, value); }
    private string filtro = "";
    public string Filtro { get => filtro; set => SetProperty(ref filtro, value); }
    public IAsyncRelayCommand AtualizarCommand { get; }
    public MatriculaListViewModel(AppServices services, ConfigurationHelper config)
    {
        this.services = services; this.config = config; loadedRevision = config.Revision;
        AtualizarCommand = new AsyncRelayCommand(() => RunAsync(SearchAsync));
    }
    public void CheckRevision()
    {
        if (loadedRevision != config.Revision)
            throw new InvalidOperationException("O banco foi alterado. Atualize a lista de matrículas antes de continuar.");
    }
    private static int Number(string query, string label, bool zero = false)
    {
        if (!int.TryParse(query, out var number) || number < (zero ? 0 : 1))
            throw new ArgumentException($"{label}: informe um número inteiro {(zero ? "maior ou igual a zero" : "positivo")}.");
        return number;
    }
    private async Task SearchAsync()
    {
        Matriculas.Clear();
        var revision = config.Revision;
        var query = Filtro.Trim();
        IEnumerable<MatriculaDto> found;
        switch (Campo)
        {
            case "Id":
                var id = Number(query, "Id da matrícula");
                var one = await services.UseAsync<IMatriculaService, MatriculaDto?>((s, ct) => s.ObterPorIdAsync(id, ct));
                found = one is null ? [] : [one];
                break;
            case "Id Aluno":
                var alunoId = Number(query, "Id do aluno");
                var aluno = await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorIdAsync(alunoId, ct));
                found = aluno is null ? [] : await services.UseAsync<IMatriculaService, IEnumerable<MatriculaDto>>((s, ct) => s.ObterPorAlunoIdAsync(alunoId, ct));
                break;
            case "CPF":
                var cpf = PessoaCadastroValidation.Digits(query);
                if (cpf.Length != 11) throw new ArgumentException("Informe os 11 dígitos do CPF do aluno.");
                var byCpf = await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorCpfAsync(cpf, ct));
                found = byCpf is null ? [] : await services.UseAsync<IMatriculaService, IEnumerable<MatriculaDto>>((s, ct) => s.ObterPorAlunoIdAsync(byCpf.Id, ct));
                break;
            case "Ativas":
                var activeAluno = query.Length == 0 ? 0 : Number(query, "Id do aluno");
                found = await services.UseAsync<IMatriculaService, IEnumerable<MatriculaDto>>((s, ct) => s.ObterAtivasAsync(activeAluno, ct));
                break;
            case "Vencendo em dias":
                var days = Number(query, "Quantidade de dias", true);
                if (days > 36500) throw new ArgumentException("Informe no máximo 36500 dias.");
                found = await services.UseAsync<IMatriculaService, IEnumerable<MatriculaDto>>((s, ct) => s.ObterVencendoEmDiasAsync(days, ct));
                break;
            default:
                found = await services.UseAsync<IMatriculaService, IEnumerable<MatriculaDto>>((s, ct) => s.ObterTodasAsync(ct));
                break;
        }
        if (revision != config.Revision) throw new InvalidOperationException("O banco foi alterado durante a consulta. Atualize a lista.");
        foreach (var item in found.OrderByDescending(x => x.Id)) Matriculas.Add(item);
        loadedRevision = revision;
        Status = Matriculas.Count + " matrícula(s) encontrada(s).";
    }
    public Task DeleteAsync(MatriculaDto item) => RunAsync(async () =>
    {
        CheckRevision();
        if (!await services.UseAsync<IMatriculaService, bool>((s, ct) => s.RemoverAsync(item.Id, ct)))
            throw new KeyNotFoundException("A matrícula já foi removida.");
        await SearchAsync();
    });
}
