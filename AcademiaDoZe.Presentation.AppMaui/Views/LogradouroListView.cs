// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public class LogradouroListView : ContentPage
{
    private readonly LogradouroListViewModel vm;
    public LogradouroListView(AppServices services, ConfigurationHelper config)
    {
        Title = "Logradouros"; BindingContext = vm = new(services);
        var picker = new Picker { Title = "Filtrar por", ItemsSource = vm.Campos }; picker.SetBinding(Picker.SelectedItemProperty, nameof(vm.Campo));
        var add = Ui.Button("Novo logradouro", "add.png");
        add.Clicked += async (_, _) => await Navigation.PushAsync(new LogradouroView(services, config, 0));
        var list = new CollectionView { SelectionMode = SelectionMode.Single, EmptyView = "Nenhum logradouro encontrado." };
        list.SetBinding(ItemsView.ItemsSourceProperty, nameof(vm.Items));
        list.ItemTemplate = new DataTemplate(() =>
        {
            var name = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold }; name.SetBinding(Label.TextProperty, "Nome");
            var detail = new Label { FontSize = 14 }; detail.SetBinding(Label.TextProperty, new Binding("Cidade", stringFormat: "Cidade: {0}"));
            var cep = new Label { FontSize = 13 }; cep.SetBinding(Label.TextProperty, new Binding("Cep", stringFormat: "CEP: {0}"));
            return new VerticalStackLayout { Padding = 12, Spacing = 4, Children = { name, detail, cep, new BoxView { HeightRequest = 1, Color = Color.FromArgb("#78909C") } } };
        });
        list.SelectionChanged += async (_, e) =>
        {
            if (e.CurrentSelection.FirstOrDefault() is not LogradouroDto dto) return;
            list.SelectedItem = null; await Navigation.PushAsync(new LogradouroView(services, config, dto.Id));
        };
        var head = Ui.Body(Ui.Title("Logradouros"), Ui.Caption("Selecione um cadastro para editar ou excluir."), add, picker,
         Ui.Field("Texto da busca", nameof(vm.Filtro)), Ui.Button("Filtrar / atualizar", "search.png", vm.AtualizarCommand), Ui.Status(), Ui.Busy());
        var grid = new Grid { RowDefinitions = { new(GridLength.Auto), new(GridLength.Star) } };
        grid.Add(head); grid.Add(list, 0, 1); Content = grid;
    }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.AtualizarCommand.ExecuteAsync(null); }
}
