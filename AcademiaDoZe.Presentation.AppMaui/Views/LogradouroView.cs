// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public class LogradouroView : ContentPage
{
    private readonly LogradouroViewModel vm;
    private bool loaded;
    public LogradouroView(AppServices services, ConfigurationHelper config, int id)
    {
        Title = id == 0 ? "Novo logradouro" : "Editar logradouro"; BindingContext = vm = new(services, config, id);
        vm.Saved += async () => await Navigation.PopAsync();
        var remove = Ui.Button("Excluir cadastro", "delete.png"); remove.BackgroundColor = Color.FromArgb("#B44838"); remove.IsVisible = id != 0;
        remove.Clicked += async (_, _) => { if (!vm.IsBusy && await DisplayAlertAsync("Excluir logradouro?", "Esta operação remove o cadastro selecionado.", "Excluir", "Cancelar")) await vm.DeleteAsync(); };
        Content = new ScrollView
        {
            Content = Ui.Body(Ui.Title(Title), Ui.Caption("Todos os campos são obrigatórios."),
         Ui.Field("CEP", nameof(vm.Cep)), Ui.Button("Buscar CEP no banco", "search.png", vm.BuscarCepCommand),
         Ui.Field("Nome", nameof(vm.Nome)), Ui.Field("Bairro", nameof(vm.Bairro)), Ui.Field("Cidade", nameof(vm.Cidade)),
         Ui.Field("UF (duas letras)", nameof(vm.Estado)), Ui.Field("País", nameof(vm.Pais)),
         Ui.Button("Salvar logradouro", "save.png", vm.SalvarCommand), remove, Ui.Busy(), Ui.Status())
        };
    }
    protected override async void OnAppearing() { base.OnAppearing(); if (!loaded) { loaded = true; await vm.LoadAsync(); } }
}
