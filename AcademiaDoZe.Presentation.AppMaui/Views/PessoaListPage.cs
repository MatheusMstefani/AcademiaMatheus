// Matheus Marques Stefani
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.Converters;
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public abstract class PessoaListPage : ContentPage
{
    private readonly PessoaListViewModel vm;
    protected PessoaListPage(AppServices services, ConfigurationHelper config, PessoaListViewModel vm)
    {
        this.vm = vm; BindingContext = vm; Title = vm.IsColaborador ? "Colaboradores" : "Alunos";
        Page Editor(int id) => vm.IsColaborador ? new ColaboradorPage(services, config, id) : new AlunoPage(services, config, id);
        var add = Ui.Button(vm.IsColaborador ? "Novo colaborador" : "Novo aluno", "add.png");
        add.Clicked += async (_, _) => await Navigation.PushAsync(Editor(0));
        var filter = new Picker { Title = "Filtrar por", ItemsSource = vm.Campos };
        filter.SetBinding(Picker.SelectedItemProperty, nameof(vm.Campo));
        var search = new Entry { Placeholder = "Digite nome, CPF ou outro filtro" };
        search.SetBinding(Entry.TextProperty, nameof(vm.Filtro), BindingMode.TwoWay);
        search.ReturnCommand = vm.AtualizarCommand;
        var controls = Ui.Body(Ui.Title(Title), Ui.Caption("Consulte, edite e exclua os cadastros. Para buscar CPF, informe todos os 11 dígitos."),
            add, filter, search, Ui.Button("Filtrar / atualizar", "search.png", vm.AtualizarCommand), Ui.Busy(), Ui.Status());
        var layout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical) { HorizontalItemSpacing = 12, VerticalItemSpacing = 12 };
        var list = new CollectionView
        {
            ItemsLayout = layout,
            SelectionMode = SelectionMode.None,
            Margin = 20,
            EmptyView = "Nenhum cadastro encontrado. Use o botão de novo cadastro."
        };
        list.SetBinding(ItemsView.ItemsSourceProperty, nameof(vm.Items));
        list.ItemTemplate = new DataTemplate(() =>
        {
            var content = new VerticalStackLayout { Padding = 16, Spacing = 7 };
            var photo = new Image
            {
                WidthRequest = DeviceInfo.Idiom == DeviceIdiom.Phone ? 96 : 120,
                HeightRequest = DeviceInfo.Idiom == DeviceIdiom.Phone ? 96 : 120,
                Aspect = Aspect.AspectFill,
                HorizontalOptions = LayoutOptions.Start
            };
            photo.SetBinding(Image.SourceProperty, new Binding("Foto", converter: new FotoToImageSourceConverter()));
            content.Children.Add(photo);
            content.Children.Add(Text("Nome", null, true));
            content.Children.Add(Text("Cpf", "CPF: {0}"));
            content.Children.Add(Text("Telefone", "Telefone: {0}"));
            content.Children.Add(Text("Email", "E-mail: {0}"));
            content.Children.Add(Text("DataNascimento", "Nascimento: {0:dd/MM/yyyy}"));
            if (vm.IsColaborador)
            {
                content.Children.Add(Text("DataAdmissao", "Admissão: {0:dd/MM/yyyy}"));
                var type = Text("Tipo", "Tipo: {0}", converter: new EnumDisplayConverter());
                var contract = Text("Vinculo", "Vínculo: {0}", converter: new EnumDisplayConverter());
                content.Children.Add(type); content.Children.Add(contract);
            }
            content.Children.Add(Text("Endereco.Nome", "Logradouro: {0}"));
            content.Children.Add(Text("Numero", "Número: {0}"));
            content.Children.Add(Text("Complemento", "Complemento: {0}"));
            var edit = Ui.Button("Editar", "edit.png");
            edit.Clicked += async (_, _) => { if (edit.BindingContext is PessoaDto dto) await Navigation.PushAsync(Editor(dto.Id)); };
            content.Children.Add(edit);
            var border = new Border { Content = content, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } };
            border.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#203A4D"));
            return border;
        });
        SizeChanged += (_, _) => layout.Span = Width >= 1250 ? 3 : Width >= 850 ? 2 : 1;
        var grid = new Grid { RowDefinitions = { new(GridLength.Auto), new(GridLength.Star) } };
        grid.Add(controls); grid.Add(list, 0, 1); Content = grid;
    }
    private static Label Text(string path, string? format, bool bold = false, IValueConverter? converter = null)
    {
        var label = new Label { FontSize = bold ? 19 : 14, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, LineBreakMode = LineBreakMode.WordWrap };
        label.SetBinding(Label.TextProperty, new Binding(path, converter: converter, stringFormat: format));
        return label;
    }
    protected override async void OnAppearing() { base.OnAppearing(); await vm.AtualizarCommand.ExecuteAsync(null); }
}
