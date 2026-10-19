// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class AlunoListPage(AppServices services, ConfigurationHelper config)
    : PessoaListPage(services, config, new AlunoListViewModel(services));
