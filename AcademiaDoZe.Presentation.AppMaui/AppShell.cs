// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.Views;
namespace AcademiaDoZe.Presentation.AppMaui;

public class AppShell : Shell
{
    public AppShell(AppServices services, ConfigurationHelper config)
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;
        FlyoutHeader = new Image { Source = "academiadoze.png", HeightRequest = 140, Margin = 16 };
        Add("Painel", "home.png", "painel", () => new DashboardListView(services));
        Add("Logradouros", "pin.png", "logradouros", () => new LogradouroListView(services, config));
        Add("Configurações", "settings.png", "configuracoes", () => new ConfiguracoesView(services, config));
    }
    private void Add(string title, string icon, string route, Func<object> factory) =>
     Items.Add(new FlyoutItem { Title = title, Icon = icon, Route = route, Items = { new ShellContent { ContentTemplate = new DataTemplate(factory) } } });
}
