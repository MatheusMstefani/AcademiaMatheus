// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public class ConfiguracoesView : ContentPage
{
    private readonly ConfiguracoesViewModel vm;
    private bool changing;
    public ConfiguracoesView(AppServices services, ConfigurationHelper config)
    {
        Title = "Configurações"; BindingContext = vm = new(config);
        var themes = new Picker { Title = "Tema", ItemsSource = vm.Temas }; themes.SetBinding(Picker.SelectedItemProperty, nameof(vm.Tema));
        var providers = new Picker { Title = "Banco", ItemsSource = vm.Bancos }; providers.SetBinding(Picker.SelectedItemProperty, nameof(vm.Banco));
        providers.SelectedIndexChanged += async (_, _) => { if (changing) return; await vm.LoadAsync(); };
        var path = Ui.Field("Caminho completo do SQLite", nameof(vm.Caminho)); path.SetBinding(IsVisibleProperty, nameof(vm.IsSqlite));
        var server = new VerticalStackLayout { Spacing = 12, Children = { Ui.Field("Servidor", nameof(vm.Servidor)), Ui.Field("Banco de dados", nameof(vm.NomeBanco)), Ui.Field("Usuário", nameof(vm.Usuario)), Ui.Field("Senha", nameof(vm.Senha), true) } };
        server.SetBinding(IsVisibleProperty, nameof(vm.IsServer));
        Content = new ScrollView
        {
            Content = Ui.Body(Ui.Title("Do seu jeito."), Ui.Caption("O tema muda imediatamente. O banco muda após testar e salvar."),
         new Label { Text = "APARÊNCIA", FontAttributes = FontAttributes.Bold }, themes,
         new Label { Text = "BANCO DE DADOS", FontAttributes = FontAttributes.Bold }, providers, path, server,
         Ui.Field("Opções complementares (opcional)", nameof(vm.Complemento)),
         Ui.Caption("SQLite: use um arquivo em uma pasta existente. Um arquivo novo será inicializado automaticamente. Senhas de servidores ficam no armazenamento seguro."),
         Ui.Button("Testar e salvar conexão", "save.png", vm.SalvarCommand), Ui.Busy(), Ui.Status())
        };
    }
    protected override async void OnAppearing() { base.OnAppearing(); changing = true; await vm.LoadAsync(); changing = false; }
}
