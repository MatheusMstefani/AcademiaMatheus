// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Converters;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public abstract class PessoaPage : ContentPage
{
    private readonly PessoaViewModel vm;
    private bool loaded;
    protected PessoaPage(PessoaViewModel vm)
    {
        this.vm = vm; BindingContext = vm;
        Title = vm.IsEditMode ? "Editar " + vm.Categoria : "Novo " + vm.Categoria;
        vm.Saved += async () => await Navigation.PopAsync();
        var cpf = new Entry { Placeholder = "000.000.000-00", Keyboard = Keyboard.Numeric, MaxLength = 14 };
        cpf.SetBinding(Entry.TextProperty, "Pessoa.Cpf", BindingMode.TwoWay);
        cpf.SetBinding(Entry.IsReadOnlyProperty, nameof(vm.IsEditMode));
        var photo = new Image { WidthRequest = 140, HeightRequest = 140, Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.Start };
        photo.SetBinding(Image.SourceProperty, new Binding("Pessoa.Foto", converter: new FotoToImageSourceConverter()));
        var address = new Label { LineBreakMode = LineBreakMode.WordWrap };
        address.SetBinding(Label.TextProperty, nameof(vm.EnderecoResumo));
        var form = Ui.Body(Ui.Title(Title), Ui.Caption("Preencha os dados e vincule um logradouro já cadastrado."),
            new Label { Text = "CPF" }, cpf, Ui.Button("Buscar CPF cadastrado", "search.png", vm.BuscarCpfCommand),
            Ui.Title("Dados pessoais"), Ui.Field("Nome completo", "Pessoa.Nome"), DateField("Data de nascimento", "Pessoa.DataNascimento"),
            Ui.Field("Telefone com DDD (11 dígitos)", "Pessoa.Telefone"), Ui.Field("E-mail", "Pessoa.Email"),
            photo, Ui.Button("Selecionar foto da galeria", "photo.png", vm.GaleriaCommand),
            Ui.Button("Tirar foto com a câmera", "camera.png", vm.CameraCommand),
            Ui.Title("Endereço"), Ui.Field("CEP cadastrado", nameof(vm.Cep)), Ui.Button("Buscar CEP", "search.png", vm.BuscarCepCommand),
            address, Ui.Field("Número", "Pessoa.Numero"), Ui.Field("Complemento (opcional)", "Pessoa.Complemento"));
        if (vm.IsColaborador)
        {
            form.Children.Add(Ui.Title("Dados de trabalho"));
            form.Children.Add(DateField("Data de admissão", "Pessoa.DataAdmissao"));
            form.Children.Add(EnumField("Tipo de colaborador", "Pessoa.Tipo", vm.Tipos));
            form.Children.Add(EnumField("Vínculo", "Pessoa.Vinculo", vm.Vinculos));
        }
        form.Children.Add(Ui.Title("Senha"));
        form.Children.Add(Ui.Caption("No cadastro, informe uma senha com 6 caracteres e uma maiúscula. Na edição, deixe ambos os campos vazios para manter a senha."));
        form.Children.Add(Ui.Field("Senha", "Pessoa.Senha", true));
        form.Children.Add(Ui.Field("Confirmar senha", nameof(vm.ConfirmarSenha), true));
        form.Children.Add(Ui.Button("Salvar cadastro", "save.png", vm.SalvarCommand));
        var cancel = Ui.Button("Cancelar e voltar", "back.png");
        cancel.Clicked += async (_, _) => { if (!vm.IsBusy) await Navigation.PopAsync(); };
        form.Children.Add(cancel);
        var remove = Ui.Button("Excluir cadastro", "delete.png"); remove.BackgroundColor = Color.FromArgb("#B44838");
        remove.SetBinding(IsVisibleProperty, nameof(vm.IsEditMode));
        remove.Clicked += async (_, _) =>
        {
            if (!vm.IsBusy && await DisplayAlertAsync("Excluir cadastro?", $"Excluir {vm.Pessoa.Nome}?", "Excluir", "Cancelar"))
                await vm.DeleteAsync();
        };
        form.Children.Add(remove);
        form.Children.Add(Ui.Busy());
        form.Children.Add(Ui.Status());
        Content = new ScrollView { Content = form };
    }
    private static View DateField(string caption, string path)
    {
        var date = new DatePicker { Format = "dd/MM/yyyy", MaximumDate = DateTime.Today, MinimumDate = new DateTime(1900, 1, 1) };
        date.SetBinding(DatePicker.DateProperty, new Binding(path, BindingMode.TwoWay, new DateOnlyToDateTimeConverter()));
        return new VerticalStackLayout { Spacing = 4, Children = { new Label { Text = caption, FontSize = 13 }, date } };
    }
    private static View EnumField(string caption, string path, Array items)
    {
        var picker = new Picker { Title = caption, ItemsSource = items, ItemDisplayBinding = new Binding(".", converter: new EnumDisplayConverter()) };
        picker.SetBinding(Picker.SelectedItemProperty, path, BindingMode.TwoWay);
        return new VerticalStackLayout { Spacing = 4, Children = { new Label { Text = caption }, picker } };
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!loaded) { loaded = true; await vm.LoadAsync(); }
    }
}
