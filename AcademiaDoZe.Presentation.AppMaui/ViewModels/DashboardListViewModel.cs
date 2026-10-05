// Matheus Marques Stefani
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel(AppServices services) : BaseViewModel
{
    private int logradouros;
    public int Logradouros { get => logradouros; set { SetProperty(ref logradouros, value); } }
    private int alunos;
    public int Alunos { get => alunos; set { SetProperty(ref alunos, value); } }
    private int colaboradores;
    public int Colaboradores { get => colaboradores; set { SetProperty(ref colaboradores, value); } }
    private int matriculas;
    public int Matriculas { get => matriculas; set { SetProperty(ref matriculas, value); } }
    [RelayCommand]
    private Task AtualizarAsync() => RunAsync(async () =>
    {
        Logradouros = Alunos = Colaboradores = Matriculas = 0;
        Logradouros = await services.UseAsync<ILogradouroService, int>(async (s, ct) => (await s.ObterTodosAsync(ct)).Count());
        Alunos = await services.UseAsync<IAlunoService, int>(async (s, ct) => (await s.ObterTodosAsync(ct)).Count());
        Colaboradores = await services.UseAsync<IColaboradorService, int>(async (s, ct) => (await s.ObterTodosAsync(ct)).Count());
        Matriculas = await services.UseAsync<IMatriculaService, int>(async (s, ct) => (await s.ObterTodasAsync(ct)).Count());
    });
}
