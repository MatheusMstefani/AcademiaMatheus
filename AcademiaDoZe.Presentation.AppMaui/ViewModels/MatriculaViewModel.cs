// Matheus Marques Stefani
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Validation;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class MatriculaViewModel : BaseViewModel
{
    private readonly AppServices services;
    private readonly ConfigurationHelper config;
    private readonly int revision;
    private bool syncing;
    public MatriculaDto Matricula { get; private set; }
    public bool IsEditMode => Matricula.Id > 0;
    public Array Planos { get; } = Enum.GetValues<AppMatriculaPlano>();
    public ObservableCollection<RestricaoSelectable> RestricoesMulti { get; } = [];
    private string cpf = "";
    public string Cpf
    {
        get => cpf;
        set
        {
            if (!SetProperty(ref cpf, value) || IsEditMode) return;
            if (PessoaCadastroValidation.Digits(value) != Matricula.AlunoMatricula.Cpf)
            { Matricula.AlunoMatricula = AlunoVazio(); Notify(); }
        }
    }
    public AppMatriculaPlano Plano
    {
        get => Matricula.Plano;
        set { if (Matricula.Plano == value) return; Matricula.Plano = value; OnPropertyChanged(); CalculateEnd(); }
    }
    public DateOnly DataInicio
    {
        get => Matricula.DataInicio;
        set { if (Matricula.DataInicio == value) return; Matricula.DataInicio = value; OnPropertyChanged(); CalculateEnd(); }
    }
    public string DataFimResumo => $"Término automático: {Matricula.DataFim:dd/MM/yyyy}";
    public string AlunoResumo => Matricula.AlunoMatricula is { Id: > 0 } a
        ? $"{a.Nome}\nCPF: {a.Cpf}\nNascimento: {a.DataNascimento:dd/MM/yyyy}\nId do aluno: {a.Id}"
        : "Nenhum aluno selecionado. Digite o CPF e clique em Buscar aluno.";
    public string RestricoesResumo => Matricula.RestricoesMedicas.GetDisplayName();
    public string LaudoResumo => Matricula.LaudoMedico?.Conteudo is { Length: > 0 }
        ? "Laudo anexado e pronto para salvar."
        : MatriculaCadastroValidation.ExigeLaudo(Matricula)
            ? "Laudo obrigatório: menor de 16 anos ou restrições selecionadas."
            : "Laudo opcional. Nenhum documento anexado.";
    public bool HasLaudo => Matricula.LaudoMedico?.Conteudo is { Length: > 0 };
    public bool CanSearchAluno => !IsEditMode;
    public IAsyncRelayCommand BuscarAlunoCommand { get; }
    public IAsyncRelayCommand SalvarCommand { get; }
    public IAsyncRelayCommand GaleriaCommand { get; }
    public IAsyncRelayCommand CameraCommand { get; }
    public IRelayCommand RemoverLaudoCommand { get; }
    public event Func<Task>? Saved;

    public MatriculaViewModel(AppServices services, ConfigurationHelper config, int id = 0)
    {
        this.services = services; this.config = config; revision = config.Revision;
        var today = DateOnly.FromDateTime(DateTime.Today);
        Matricula = new() { Id = id, AlunoMatricula = AlunoVazio(), Plano = AppMatriculaPlano.Mensal,
            DataInicio = today, DataFim = today.AddMonths(1), Objetivo = "", RestricoesMedicas = AppMatriculaRestricoes.None };
        foreach (var flag in Enum.GetValues<AppMatriculaRestricoes>().Where(x => x != AppMatriculaRestricoes.None))
        {
            var item = new RestricaoSelectable(flag);
            item.PropertyChanged += (_, e) =>
            {
                if (syncing || e.PropertyName != nameof(item.IsSelected)) return;
                Matricula.RestricoesMedicas = RestricoesMulti.Where(x => x.IsSelected)
                    .Aggregate(AppMatriculaRestricoes.None, (a, b) => a | b.Value);
                OnPropertyChanged(nameof(RestricoesResumo)); OnPropertyChanged(nameof(LaudoResumo));
            };
            RestricoesMulti.Add(item);
        }
        BuscarAlunoCommand = new AsyncRelayCommand(() => RunAsync(SearchAlunoAsync));
        SalvarCommand = new AsyncRelayCommand(() => RunAsync(SaveAsync));
        GaleriaCommand = new AsyncRelayCommand(() => RunAsync(() => SelectLaudoAsync(false)));
        CameraCommand = new AsyncRelayCommand(() => RunAsync(() => SelectLaudoAsync(true)));
        RemoverLaudoCommand = new RelayCommand(() =>
        {
            if (IsBusy) return;
            Matricula.LaudoMedico = null; Matricula.RemoverLaudoMedico = IsEditMode;
            Notify(); Status = "Laudo removido do formulário. Salve para confirmar a alteração.";
        });
    }
    private static AlunoDto AlunoVazio() => new() { Nome = "", Cpf = "", DataNascimento = default, Telefone = "", Numero = "" };
    private void CheckRevision()
    {
        if (revision != config.Revision) throw new InvalidOperationException("O banco foi alterado. Volte à lista e reabra a matrícula.");
    }
    private void CalculateEnd()
    {
        Matricula.DataFim = MatriculaCadastroValidation.CalcularDataFim(Plano, DataInicio);
        OnPropertyChanged(nameof(DataFimResumo));
    }
    private void Notify()
    {
        OnPropertyChanged(nameof(Matricula)); OnPropertyChanged(nameof(AlunoResumo));
        OnPropertyChanged(nameof(RestricoesResumo)); OnPropertyChanged(nameof(LaudoResumo)); OnPropertyChanged(nameof(HasLaudo));
        OnPropertyChanged(nameof(IsEditMode)); OnPropertyChanged(nameof(CanSearchAluno));
    }
    private void Fill(MatriculaDto dto)
    {
        Matricula = dto; cpf = dto.AlunoMatricula.Cpf;
        syncing = true;
        try { foreach (var item in RestricoesMulti) item.IsSelected = dto.RestricoesMedicas.HasFlag(item.Value); }
        finally { syncing = false; }
        OnPropertyChanged(nameof(Cpf)); OnPropertyChanged(nameof(Plano)); OnPropertyChanged(nameof(DataInicio));
        CalculateEnd(); Notify();
    }
    public Task LoadAsync() => RunAsync(async () =>
    {
        CheckRevision();
        if (!IsEditMode) return;
        Fill(await services.UseAsync<IMatriculaService, MatriculaDto?>((s, ct) => s.ObterPorIdAsync(Matricula.Id, ct))
            ?? throw new KeyNotFoundException("Matrícula não encontrada."));
    });
    private async Task SearchAlunoAsync()
    {
        CheckRevision();
        if (IsEditMode) throw new InvalidOperationException("O aluno da matrícula não pode ser alterado.");
        var digits = PessoaCadastroValidation.Digits(Cpf);
        if (digits.Length != 11) throw new ArgumentException("Digite os 11 dígitos do CPF do aluno.");
        Matricula.AlunoMatricula = await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorCpfAsync(digits, ct)) ?? AlunoVazio();
        Notify();
        if (Matricula.AlunoMatricula.Id == 0) { Status = "Aluno não encontrado. Cadastre-o na tela Alunos."; return; }
        var active = await services.UseAsync<IMatriculaService, bool>((s, ct) => s.PossuiMatriculaAtivaAsync(Matricula.AlunoMatricula.Id, ct));
        Status = active ? "Este aluno já tem matrícula ativa. Uma nova matrícula será bloqueada." : "Aluno selecionado.";
    }
    private async Task SelectLaudoAsync(bool camera)
    {
        CheckRevision();
        var file = await PhotoService.SelectAsync(camera, "Selecione a imagem do laudo médico");
        if (file is null) return;
        Matricula.LaudoMedico = file; Matricula.RemoverLaudoMedico = false; Notify();
        Status = "Laudo selecionado. Salve a matrícula para gravar o documento.";
    }
    private async Task SaveAsync()
    {
        CheckRevision();
        // Reconsulta a idade e os dados reais; o formulário não pode burlar as regras.
        if (Matricula.AlunoMatricula.Id > 0)
            Matricula.AlunoMatricula = await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorIdAsync(Matricula.AlunoMatricula.Id, ct))
                ?? throw new InvalidOperationException("O aluno selecionado não existe mais.");
        Notify();
        var errors = MatriculaCadastroValidation.Validate(Matricula);
        if (errors.Count > 0) throw new ArgumentException(string.Join(Environment.NewLine, errors));
        CalculateEnd();
        var result = await services.UseAsync<IMatriculaService, MatriculaDto>((s, ct) =>
            IsEditMode ? s.AtualizarAsync(Matricula, ct) : s.AdicionarAsync(Matricula, ct));
        Fill(result); Status = "Matrícula salva.";
        if (Saved is not null) await Saved();
    }
    public Task DeleteAsync() => RunAsync(async () =>
    {
        CheckRevision();
        if (!IsEditMode) return;
        if (!await services.UseAsync<IMatriculaService, bool>((s, ct) => s.RemoverAsync(Matricula.Id, ct)))
            throw new KeyNotFoundException("A matrícula já foi removida.");
        if (Saved is not null) await Saved();
    });
}
