// Matheus Marques Stefani
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Validation;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public abstract class PessoaListViewModel : BaseViewModel
{
    private readonly AppServices services;
    public bool IsColaborador { get; }
    public ObservableCollection<PessoaDto> Items { get; } = [];
    public string[] Campos { get; }
    private string campo = "Nome";
    public string Campo { get => campo; set => SetProperty(ref campo, value); }
    private string filtro = "";
    public string Filtro { get => filtro; set => SetProperty(ref filtro, value); }
    public IAsyncRelayCommand AtualizarCommand { get; }
    protected PessoaListViewModel(AppServices services, bool employee)
    {
        this.services = services; IsColaborador = employee;
        Campos = employee ? ["Nome", "CPF", "Id", "Email", "Tipo", "Vínculo"] : ["Nome", "CPF", "Id", "Email"];
        AtualizarCommand = new AsyncRelayCommand(() => RunAsync(SearchAsync));
    }
    private async Task SearchAsync()
    {
        Items.Clear();
        var query = Filtro.Trim();
        IEnumerable<PessoaDto> found;
        if (query.Length > 0 && Campo == "CPF")
        {
            var cpf = PessoaCadastroValidation.Digits(query);
            if (cpf.Length != 11) throw new ArgumentException("Para buscar por CPF, informe 11 dígitos.");
            PessoaDto? one = IsColaborador
                ? await services.UseAsync<IColaboradorService, ColaboradorDto?>((s, ct) => s.ObterPorCpfAsync(cpf, ct))
                : await services.UseAsync<IAlunoService, AlunoDto?>((s, ct) => s.ObterPorCpfAsync(cpf, ct));
            found = one is null ? [] : [one];
        }
        else
        {
            if (query.Length > 0 && Campo == "Id" && (!int.TryParse(query, out var parsed) || parsed <= 0))
                throw new ArgumentException("Informe um Id inteiro positivo.");
            found = IsColaborador
                ? await services.UseAsync<IColaboradorService, IEnumerable<ColaboradorDto>>((s, ct) => s.ObterTodosAsync(ct))
                : await services.UseAsync<IAlunoService, IEnumerable<AlunoDto>>((s, ct) => s.ObterTodosAsync(ct));
            found = found.Where(x => query.Length == 0 || Campo switch
            {
                "Id" => x.Id.ToString() == query,
                "Email" => string.Equals(x.Email, query, StringComparison.OrdinalIgnoreCase),
                "Tipo" => x is ColaboradorDto c && (Contains(c.Tipo.ToString(), query) || Contains(c.Tipo.GetDisplayName(), query)),
                "Vínculo" => x is ColaboradorDto c && (Contains(c.Vinculo.ToString(), query) || Contains(c.Vinculo.GetDisplayName(), query)),
                _ => Contains(x.Nome, query)
            });
        }
        foreach (var item in found.OrderBy(x => x.Nome)) Items.Add(item);
        Status = Items.Count + " cadastro(s) encontrado(s).";
    }
    private static bool Contains(string text, string query) => text.Contains(query, StringComparison.OrdinalIgnoreCase);
}
