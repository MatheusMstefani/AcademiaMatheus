// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public class DashboardListView : ContentPage
{
    private readonly DashboardListViewModel vm;
    public DashboardListView(AppServices services)
    {
        Title = "Painel"; BindingContext = vm = new(services);
        var cards = new Grid { ColumnDefinitions = { new(GridLength.Star), new(GridLength.Star) }, RowDefinitions = { new(GridLength.Auto), new(GridLength.Auto) }, ColumnSpacing = 16, RowSpacing = 16 };
        string[] names = ["Logradouros", "Alunos", "Colaboradores", "Matriculas"];
        for (var i = 0; i < names.Length; i++)
        {
            var name = names[i]; var card = Ui.Button(name, "pin.png");
            card.HeightRequest = 110; card.FontSize = 22;
            card.SetBinding(Button.TextProperty, new Binding(name, stringFormat: name + "  •  {0}"));
            card.Clicked += async (_, _) => { if (name == "Logradouros") await Shell.Current.GoToAsync("//logradouros"); else await DisplayAlertAsync(name, "Total consultado no banco atual. O cadastro desta área será desenvolvido nas próximas atividades.", "Entendi"); };
            cards.Add(card, i % 2, i / 2);
        }
        Content = new ScrollView
        {
            Content = Ui.Body(new Image { Source = "academiadoze.png", HeightRequest = 155, HorizontalOptions = LayoutOptions.Start },
         Ui.Title("Tudo pronto para um novo dia."), Ui.Caption("Seu painel de gestão • dados do banco selecionado"), cards,
         Ui.Button("Atualizar números", "refresh.png", vm.AtualizarCommand), Ui.Busy(), Ui.Status())
        };
    }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.AtualizarCommand.ExecuteAsync(null); }
}
