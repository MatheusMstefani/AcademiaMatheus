// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroViewModel(AppServices services, ConfigurationHelper config, int id) : BaseViewModel
{
    private readonly int revision = config.Revision;
    private int registroId = id;
    public int RegistroId { get => registroId; set { SetProperty(ref registroId, value); } }
    private string cep = "";
    public string Cep { get => cep; set { SetProperty(ref cep, value); } }
    private string nome = "";
    public string Nome { get => nome; set { SetProperty(ref nome, value); } }
    private string bairro = "";
    public string Bairro { get => bairro; set { SetProperty(ref bairro, value); } }
    private string cidade = "";
    public string Cidade { get => cidade; set { SetProperty(ref cidade, value); } }
    private string estado = "";
    public string Estado { get => estado; set { SetProperty(ref estado, value); } }
    private string pais = "Brasil";
    public string Pais { get => pais; set { SetProperty(ref pais, value); } }
    public event Func<Task>? Saved;
    public Task LoadAsync() => RunAsync(async () =>
    {
        if (RegistroId == 0) return;
        var dto = await services.UseAsync<ILogradouroService, LogradouroDto?>((s, ct) => s.ObterPorIdAsync(RegistroId, ct));
        if (dto is null) throw new InvalidOperationException("Registro não encontrado.");
        Fill(dto);
    });
    private void Fill(LogradouroDto dto)
    {
        RegistroId = dto.Id; Cep = dto.Cep; Nome = dto.Nome; Bairro = dto.Bairro; Cidade = dto.Cidade; Estado = dto.Estado; Pais = dto.Pais;
    }
    private void CheckRevision()
    {
        if (revision != config.Revision) throw new InvalidOperationException("O banco foi alterado. Volte à lista e abra o cadastro novamente.");
    }
    [RelayCommand]
    private Task BuscarCepAsync() => RunAsync(async () =>
    {
        CheckRevision();
        var dto = await services.UseAsync<ILogradouroService, LogradouroDto?>((s, ct) => s.ObterPorCepAsync(Cep, ct));
        if (dto is null) Status = "CEP ainda não cadastrado neste banco. Preencha os dados.";
        else { Fill(dto); Status = "Cadastro existente carregado para edição."; }
    });
    [RelayCommand]
    private Task SalvarAsync() => RunAsync(async () =>
    {
        CheckRevision(); var errors = new List<string>();
        if (new string(Cep.Where(char.IsDigit).ToArray()).Length != 8) errors.Add("CEP deve ter 8 dígitos.");
        if (string.IsNullOrWhiteSpace(Nome)) errors.Add("Informe o nome.");
        if (string.IsNullOrWhiteSpace(Bairro)) errors.Add("Informe o bairro.");
        if (string.IsNullOrWhiteSpace(Cidade)) errors.Add("Informe a cidade.");
        if (Estado.Trim().Length != 2 || !Estado.Trim().All(char.IsLetter)) errors.Add("UF deve ter duas letras.");
        if (string.IsNullOrWhiteSpace(Pais)) errors.Add("Informe o país.");
        if (errors.Count > 0) throw new ArgumentException(string.Join(Environment.NewLine, errors));
        var dto = new LogradouroDto { Id = RegistroId, Cep = Cep, Nome = Nome, Bairro = Bairro, Cidade = Cidade, Estado = Estado, Pais = Pais };
        Fill(await services.UseAsync<ILogradouroService, LogradouroDto>((s, ct) => RegistroId == 0 ? s.AdicionarAsync(dto, ct) : s.AtualizarAsync(dto, ct)));
        Status = "Logradouro salvo."; if (Saved is not null) await Saved();
    });
    public Task DeleteAsync() => RunAsync(async () =>
    {
        CheckRevision(); if (RegistroId == 0) return;
        if (!await services.UseAsync<ILogradouroService, bool>((s, ct) => s.RemoverAsync(RegistroId, ct))) throw new InvalidOperationException("O registro já foi removido.");
        if (Saved is not null) await Saved();
    });
}
