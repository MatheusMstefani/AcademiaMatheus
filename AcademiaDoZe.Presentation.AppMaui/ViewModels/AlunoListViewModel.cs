// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class AlunoListViewModel(AppServices services) : PessoaListViewModel(services, false);
