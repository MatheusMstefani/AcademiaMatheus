// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class ColaboradorPage(AppServices services, ConfigurationHelper config, int id = 0)
    : PessoaPage(new ColaboradorViewModel(services, config, id));
