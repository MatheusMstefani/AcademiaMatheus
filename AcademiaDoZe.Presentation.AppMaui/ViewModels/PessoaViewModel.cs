// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Validation;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public abstract class PessoaViewModel : BaseViewModel
{
    private readonly AppServices services;
    private readonly ConfigurationHelper config;
    private readonly bool employee;
    private readonly int revision;
    private PessoaDto pessoa;
    public PessoaDto Pessoa { get => pessoa; private set { SetProperty(ref pessoa, value); OnPropertyChanged(nameof(IsEditMode)); } }
    public bool IsEditMode => Pessoa.Id > 0;
    public bool IsColaborador => employee;
    public string Categoria => employee ? "colaborador" : "aluno";
    private string confirmarSenha = "";
    public string ConfirmarSenha { get => confirmarSenha; set => SetProperty(ref confirmarSenha, value); }
    private string cep = "";
    public string Cep
    {
        get => cep;
        set
        {
            if (!SetProperty(ref cep, value)) return;
            if (Pessoa.Endereco is not null && PessoaCadastroValidation.Digits(value) != Pessoa.Endereco.Cep)
            { Pessoa.Endereco = null; OnPropertyChanged(nameof(Pessoa)); OnPropertyChanged(nameof(EnderecoResumo)); }
        }
    }
    public string EnderecoResumo => Pessoa.Endereco is { Id: > 0 } e
        ? $"{e.Nome} • {e.Bairro}\n{e.Cidade} / {e.Estado} • {e.Pais}"
        : "Nenhum endereço vinculado. Cadastre um logradouro e busque seu CEP.";
    public Array Tipos { get; } = Enum.GetValues<AppColaboradorTipo>();
    public Array Vinculos { get; } = Enum.GetValues<AppColaboradorVinculo>();
    public IAsyncRelayCommand SalvarCommand { get; }
    public IAsyncRelayCommand BuscarCpfCommand { get; }
    public IAsyncRelayCommand BuscarCepCommand { get; }
    public IAsyncRelayCommand GaleriaCommand { get; }
    public IAsyncRelayCommand CameraCommand { get; }
    public event Func<Task>? Saved;
    protected PessoaViewModel(AppServices services, ConfigurationHelper config, bool employee, int id)
    {
        this.services = services; this.config = config; this.employee = employee; revision = config.Revision;
        pessoa = employee
            ? new ColaboradorDto
            {
                Nome = "",
                Cpf = "",
                Telefone = "",
                Numero = "",
                DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                DataAdmissao = DateOnly.FromDateTime(DateTime.Today),
                Tipo = AppColaboradorTipo.Atendente,
                Vinculo = AppColaboradorVinculo.CLT
            }
            : new AlunoDto { Nome = "", Cpf = "", Telefone = "", Numero = "", DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)) };
        pessoa.Id = id;
        SalvarCommand = new AsyncRelayCommand(() => RunAsync(SaveAsync));
        BuscarCpfCommand = new AsyncRelayCommand(() => RunAsync(SearchCpfAsync));
        BuscarCepCommand = new AsyncRelayCommand(() => RunAsync(SearchCepAsync));
        GaleriaCommand = new AsyncRelayCommand(() => RunAsync(() => PhotoAsync(false)));
        CameraCommand = new AsyncRelayCommand(() => RunAsync(() => PhotoAsync(true)));
    }
    private void CheckRevision()
    {
        if (revision != config.Revision) throw new InvalidOperationException("O banco foi alterado. Volte à lista e reabra o formulário.");
    }
    private void Fill(PessoaDto value)
    {
        value.Senha = null;
        Pessoa = value;
        // Não usar o setter aqui, para não invalidar o endereço recém-carregado.
        cep = value.Endereco?.Cep ?? "";
        ConfirmarSenha = "";
        OnPropertyChanged(nameof(Cep)); OnPropertyChanged(nameof(EnderecoResumo));
    }
    public Task LoadAsync() => RunAsync(async () =>
    {
        CheckRevision();
        if (Pessoa.Id == 0) return;
        PessoaDto? found = employee
            ? await services.UseAsync<IColaboradorService, ColaboradorDto?>((s, ct) => s.ObterPorIdAsync(Pessoa.Id, ct))
            : await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorIdAsync(Pessoa.Id, ct));
        Fill(found ?? throw new KeyNotFoundException("Cadastro não encontrado."));
    });
    private async Task SearchCpfAsync()
    {
        CheckRevision();
        var value = PessoaCadastroValidation.Digits(Pessoa.Cpf);
        if (value.Length != 11) throw new ArgumentException("Digite os 11 dígitos do CPF.");
        PessoaDto? found = employee
            ? await services.UseAsync<IColaboradorService, ColaboradorDto?>((s, ct) => s.ObterPorCpfAsync(value, ct))
            : await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorCpfAsync(value, ct));
        if (found is null) { Status = "CPF ainda não cadastrado."; return; }
        Fill(found); Status = "Cadastro existente carregado para edição.";
    }
    private async Task SearchCepAsync()
    {
        CheckRevision();
        var value = PessoaCadastroValidation.Digits(Cep);
        if (value.Length != 8) throw new ArgumentException("Digite os 8 dígitos do CEP.");
        var found = await services.UseAsync<ILogradouroService, LogradouroDto?>((s, ct) => s.ObterPorCepAsync(value, ct));
        Pessoa.Endereco = found;
        OnPropertyChanged(nameof(Pessoa)); OnPropertyChanged(nameof(EnderecoResumo));
        Status = found is null ? "CEP não cadastrado. Cadastre em Logradouros e volte aqui." : "Endereço vinculado.";
    }
    private async Task PhotoAsync(bool camera)
    {
        var photo = await PhotoService.SelectAsync(camera);
        if (photo is null) return;
        Pessoa.Foto = photo; OnPropertyChanged(nameof(Pessoa)); Status = "Foto selecionada.";
    }
    private async Task SaveAsync()
    {
        CheckRevision();
        var errors = PessoaCadastroValidation.Validate(Pessoa, ConfirmarSenha);
        if (errors.Count > 0) throw new ArgumentException(string.Join(Environment.NewLine, errors));
        PessoaDto saved;
        if (employee)
        {
            var dto = (ColaboradorDto)Pessoa;
            saved = await services.UseAsync<IColaboradorService, ColaboradorDto>((s, ct) => IsEditMode ? s.AtualizarAsync(dto, ct) : s.AdicionarAsync(dto, ct));
        }
        else
        {
            var dto = (AlunoDto)Pessoa;
            saved = await services.UseAsync<IAlunoService, AlunoDto>((s, ct) => IsEditMode ? s.AtualizarAsync(dto, ct) : s.AdicionarAsync(dto, ct));
        }
        Fill(saved);
        Status = "Cadastro salvo.";
        if (Saved is not null) await Saved();
    }
    public Task DeleteAsync() => RunAsync(async () =>
    {
        CheckRevision();
        if (!IsEditMode) return;
        var removed = employee
            ? await services.UseAsync<IColaboradorService, bool>((s, ct) => s.RemoverAsync(Pessoa.Id, ct))
            : await services.UseAsync<IAlunoService, bool>((s, ct) => s.RemoverAsync(Pessoa.Id, ct));
        if (!removed) throw new KeyNotFoundException("O cadastro já foi removido.");
        if (Saved is not null) await Saved();
    });
}
