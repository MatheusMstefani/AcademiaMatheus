// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Converters;
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class MatriculaPage : ContentPage
{
    private readonly MatriculaViewModel vm;
    private bool loaded;
    public MatriculaPage(AppServices services, ConfigurationHelper config, int id = 0)
    {
        BindingContext = vm = new MatriculaViewModel(services, config, id);
        Title = id == 0 ? "Nova matrícula" : "Editar matrícula";
        vm.Saved += async () => await Navigation.PopAsync();
        var cpf = new Entry { Placeholder = "CPF do aluno cadastrado", Keyboard = Keyboard.Numeric, MaxLength = 14 };
        cpf.SetBinding(Entry.TextProperty, nameof(vm.Cpf), BindingMode.TwoWay);
        cpf.SetBinding(Entry.IsReadOnlyProperty, nameof(vm.IsEditMode));
        var search = Ui.Button("Buscar aluno pelo CPF", "search.png", vm.BuscarAlunoCommand);
        search.SetBinding(IsVisibleProperty, nameof(vm.CanSearchAluno));
        var student = new Label { FontSize = 17, LineBreakMode = LineBreakMode.WordWrap };
        student.SetBinding(Label.TextProperty, nameof(vm.AlunoResumo));
        var photo = new Image { WidthRequest = 110, HeightRequest = 110, Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.Start };
        photo.SetBinding(Image.SourceProperty, new Binding("Matricula.AlunoMatricula.Foto", converter: new FotoToImageSourceConverter()));
        // Empilha os dados em janelas estreitas para não esconder nome/CPF.
        var studentPanel = new FlexLayout { Direction = Microsoft.Maui.Layouts.FlexDirection.Row, Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        studentPanel.Children.Add(photo); studentPanel.Children.Add(student);
        student.Margin = new Thickness(14, 6);
        var plans = new Picker { ItemsSource = vm.Planos, ItemDisplayBinding = new Binding(".", converter: new EnumDisplayConverter()) };
        plans.SetBinding(Picker.SelectedItemProperty, nameof(vm.Plano), BindingMode.TwoWay);
        var start = new DatePicker { Format = "dd/MM/yyyy", MinimumDate = new DateTime(1900, 1, 1), MaximumDate = new DateTime(2100, 1, 1) };
        start.SetBinding(DatePicker.DateProperty, new Binding(nameof(vm.DataInicio), BindingMode.TwoWay, new DateOnlyToDateTimeConverter()));
        var end = new Label { FontSize = 19, FontAttributes = FontAttributes.Bold };
        end.SetBinding(Label.TextProperty, nameof(vm.DataFimResumo));
        var back = Ui.Button("Voltar", "back.png");
        back.HorizontalOptions = LayoutOptions.Start;
        back.Clicked += async (_, _) => { if (!vm.IsBusy) await Navigation.PopAsync(); };
        var form = Ui.Body(back, Ui.Title(Title), Ui.Caption("Vincule um aluno, escolha a vigência e registre os cuidados de saúde."),
            Section("Aluno", new Label { Text = "CPF" }, cpf, search, studentPanel),
            Section("Plano e vigência", new Label { Text = "Plano" }, plans, new Label { Text = "Data de início" }, start, end,
                Ui.Caption("O término é calculado pelo plano e não pode ser digitado manualmente."),
                TextField("Objetivo da matrícula", "Matricula.Objetivo", 200)));
        var restrictions = new VerticalStackLayout { Spacing = 4 };
        foreach (var item in vm.RestricoesMulti)
        {
            var check = new CheckBox { BindingContext = item, Color = Color.FromArgb("#007E80") };
            check.SetBinding(CheckBox.IsCheckedProperty, nameof(item.IsSelected), BindingMode.TwoWay);
            SemanticProperties.SetDescription(check, item.Nome);
            var row = new HorizontalStackLayout { Spacing = 6, Children = { check, new Label { Text = item.Nome, VerticalOptions = LayoutOptions.Center, FontSize = 15 } } };
            restrictions.Children.Add(row);
        }
        var summary = new Label { FontSize = 14 }; summary.SetBinding(Label.TextProperty, nameof(vm.RestricoesResumo));
        form.Children.Add(Section("Condições de saúde", Ui.Caption("Marque todas as condições aplicáveis. Sem marcações = nenhuma restrição."),
            restrictions, summary, TextField("Observações e recomendações (opcional)", "Matricula.ObservacoesRestricoes", 500)));
        var report = new Image { WidthRequest = 160, HeightRequest = 120, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Start };
        report.SetBinding(IsVisibleProperty, nameof(vm.HasLaudo));
        report.SetBinding(Image.SourceProperty, new Binding("Matricula.LaudoMedico", converter: new FotoToImageSourceConverter()));
        var reportStatus = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.WordWrap };
        reportStatus.SetBinding(Label.TextProperty, nameof(vm.LaudoResumo));
        var removeReport = Ui.Button("Remover laudo", "delete.png", vm.RemoverLaudoCommand);
        removeReport.SetBinding(IsVisibleProperty, nameof(vm.HasLaudo));
        form.Children.Add(Section("Laudo médico", reportStatus, report,
            Ui.Button("Selecionar imagem do laudo", "photo.png", vm.GaleriaCommand),
            Ui.Button("Fotografar laudo", "camera.png", vm.CameraCommand), removeReport,
            Ui.Caption("Use JPEG, PNG, GIF ou BMP de até 15 MB. A imagem é salva junto à matrícula.")));
        form.Children.Add(Ui.Busy()); form.Children.Add(Ui.Status());
        form.Children.Add(Ui.Button("Salvar matrícula", "save.png", vm.SalvarCommand));
        var cancel = Ui.Button("Cancelar e voltar", "back.png");
        cancel.Clicked += async (_, _) => { if (!vm.IsBusy) await Navigation.PopAsync(); };
        form.Children.Add(cancel);
        var delete = Ui.Button("Excluir matrícula", "delete.png"); delete.BackgroundColor = Color.FromArgb("#B44838");
        delete.SetBinding(IsVisibleProperty, nameof(vm.IsEditMode));
        delete.Clicked += async (_, _) =>
        {
            if (!vm.IsBusy && await DisplayAlertAsync("Excluir matrícula?", $"Excluir a matrícula {vm.Matricula.Id} de {vm.Matricula.AlunoMatricula.Nome}?", "Excluir", "Cancelar"))
                await vm.DeleteAsync();
        };
        form.Children.Add(delete);
        Content = new ScrollView { Content = form };
    }
    private static View TextField(string caption, string path, int max)
    {
        var entry = new Entry { Placeholder = caption, MaxLength = max };
        entry.SetBinding(Entry.TextProperty, path, BindingMode.TwoWay);
        return new VerticalStackLayout { Spacing = 4, Children = { new Label { Text = caption, FontSize = 13 }, entry } };
    }
    private static Border Section(string title, params View[] views)
    {
        var content = new VerticalStackLayout { Padding = 18, Spacing = 12 };
        content.Children.Add(new Label { Text = title, FontSize = 22, FontAttributes = FontAttributes.Bold });
        foreach (var view in views) content.Children.Add(view);
        var border = new Border { Content = content, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 } };
        border.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#203A4D"));
        border.SetAppTheme<Brush>(Border.StrokeProperty, new SolidColorBrush(Color.FromArgb("#D4E2E8")), new SolidColorBrush(Color.FromArgb("#385367")));
        return border;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!loaded) { loaded = true; await vm.LoadAsync(); }
    }
}
