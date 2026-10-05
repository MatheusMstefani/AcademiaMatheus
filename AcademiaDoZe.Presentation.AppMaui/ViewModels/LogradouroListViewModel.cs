// Matheus Marques Stefani
using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel(AppServices services) : BaseViewModel
{
    public ObservableCollection<LogradouroDto> Items { get; } = [];
    public string[] Campos { get; } = ["Nome", "Cidade", "CEP", "Id"];
    private string campo = "Nome";
    public string Campo { get => campo; set { SetProperty(ref campo, value); } }
    private string filtro = "";
    public string Filtro { get => filtro; set { SetProperty(ref filtro, value); } }
    [RelayCommand]
    private Task AtualizarAsync() => RunAsync(async () =>
    {
        Items.Clear();
        var items = await services.UseAsync<ILogradouroService, IEnumerable<LogradouroDto>>((s, ct) => s.ObterTodosAsync(ct));
        var query = Filtro.Trim();
        foreach (var item in items.Where(x => string.IsNullOrEmpty(query) || (Campo switch
        {
            "Id" => x.Id.ToString() == query,
            "CEP" => new string(x.Cep.Where(char.IsDigit).ToArray()).Contains(new string(query.Where(char.IsDigit).ToArray())),
            "Cidade" => x.Cidade.Contains(query, StringComparison.OrdinalIgnoreCase),
            _ => x.Nome.Contains(query, StringComparison.OrdinalIgnoreCase)
        })).OrderBy(x => x.Nome)) Items.Add(item);
        Status = Items.Count + " logradouro(s) encontrado(s).";
    });
}
