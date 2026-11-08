// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.Converters;
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class MatriculaListPage : ContentPage
{
    private readonly MatriculaListViewModel vm;
    public MatriculaListPage(AppServices services, ConfigurationHelper config)
    {
        BindingContext = vm = new MatriculaListViewModel(services, config); Title = "Matrículas";
        var add = Ui.Button("Nova matrícula", "add.png");
        async Task Edit(int id)
        {
            if (vm.IsBusy) return;
            try { vm.CheckRevision(); await Navigation.PushAsync(new MatriculaPage(services, config, id)); }
            catch (Exception ex) { vm.Status = AppServices.Error(ex); }
        }
        add.Clicked += async (_, _) => await Edit(0);
        var filter = new Picker { Title = "Filtrar por", ItemsSource = vm.Campos };
        filter.SetBinding(Picker.SelectedItemProperty, nameof(vm.Campo), BindingMode.TwoWay);
        var query = new Entry { Placeholder = "Id, CPF, id do aluno ou quantidade de dias", ReturnCommand = vm.AtualizarCommand };
        query.SetBinding(Entry.TextProperty, nameof(vm.Filtro), BindingMode.TwoWay);
        var filters = new Grid { ColumnDefinitions = { new(new GridLength(2, GridUnitType.Star)), new(new GridLength(3, GridUnitType.Star)) }, ColumnSpacing = 12 };
        filters.Add(filter); filters.Add(query, 1);
        var back = Ui.Button("Voltar", "back.png");
        back.HorizontalOptions = LayoutOptions.Start;
        back.Clicked += async (_, _) => { if (!vm.IsBusy) await Shell.Current.GoToAsync("//painel"); };
        var header = Ui.Body(back, Ui.Title("Matrículas"), Ui.Caption("Planos, vigências e cuidados de saúde em um só lugar."),
            add, filters, Ui.Caption("Ativas: deixe vazio para todas ou informe o id do aluno. Vencendo em dias: informe 0 ou mais."),
            Ui.Button("Buscar / atualizar", "search.png", vm.AtualizarCommand), Ui.Busy(), Ui.Status());
        var layout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical) { HorizontalItemSpacing = 14, VerticalItemSpacing = 14 };
        var list = new CollectionView { ItemsLayout = layout, SelectionMode = SelectionMode.None, Margin = new Thickness(24, 0, 24, 24), EmptyView = "Nenhuma matrícula encontrada. Cadastre uma nova ou ajuste os filtros." };
        list.SetBinding(ItemsView.ItemsSourceProperty, nameof(vm.Matriculas));
        list.ItemTemplate = new DataTemplate(() =>
        {
            var content = new VerticalStackLayout { Padding = 18, Spacing = 8 };
            var photo = new Image { WidthRequest = 96, HeightRequest = 96, Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.Start };
            photo.SetBinding(Image.SourceProperty, new Binding("AlunoMatricula.Foto", converter: new FotoToImageSourceConverter()));
            content.Children.Add(photo);
            content.Children.Add(Text("AlunoMatricula.Nome", null, true));
            content.Children.Add(Text("Id", "Matrícula #{0}"));
            content.Children.Add(Text("AlunoMatricula.Cpf", "CPF: {0}"));
            content.Children.Add(Text("AlunoMatricula.DataNascimento", "Nascimento: {0:dd/MM/yyyy}"));
            content.Children.Add(Text("AlunoMatricula.Id", "Id do aluno: {0}"));
            content.Children.Add(Text("Plano", "Plano: {0}", converter: new EnumDisplayConverter()));
            content.Children.Add(Text("DataInicio", "Início: {0:dd/MM/yyyy}"));
            content.Children.Add(Text("DataFim", "Término: {0:dd/MM/yyyy}"));
            content.Children.Add(Text("Objetivo", "Objetivo: {0}"));
            content.Children.Add(Text("RestricoesMedicas", "Restrições: {0}", converter: new RestricoesFlagsDisplayConverter()));
            content.Children.Add(Text("ObservacoesRestricoes", "Observações: {0}"));
            var edit = Ui.Button("Editar matrícula", "edit.png");
            edit.Clicked += async (_, _) => { if (edit.BindingContext is MatriculaDto dto) await Edit(dto.Id); };
            content.Children.Add(edit);
            var delete = Ui.Button("Excluir", "delete.png"); delete.BackgroundColor = Color.FromArgb("#B44838");
            delete.Clicked += async (_, _) =>
            {
                if (!vm.IsBusy && delete.BindingContext is MatriculaDto dto &&
                    await DisplayAlertAsync("Excluir matrícula?", $"Excluir a matrícula {dto.Id} de {dto.AlunoMatricula.Nome}?", "Excluir", "Cancelar"))
                    await vm.DeleteAsync(dto);
            };
            content.Children.Add(delete);
            var card = new Border { Content = content, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 } };
            card.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#203A4D"));
            return card;
        });
        SizeChanged += (_, _) => layout.Span = Width >= 1350 ? 3 : Width >= 900 ? 2 : 1;
        var grid = new Grid { RowDefinitions = { new(GridLength.Auto), new(GridLength.Star) } };
        grid.Add(header); grid.Add(list, 0, 1); Content = grid;
    }
    private static Label Text(string path, string? format, bool bold = false, IValueConverter? converter = null)
    {
        var label = new Label { FontSize = bold ? 19 : 14, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, LineBreakMode = LineBreakMode.WordWrap };
        label.SetBinding(Label.TextProperty, new Binding(path, converter: converter, stringFormat: format));
        return label;
    }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.AtualizarCommand.ExecuteAsync(null); }
}
