// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class ColaboradorViewModel(AppServices services, ConfigurationHelper config, int id = 0)
    : PessoaViewModel(services, config, true, id);
