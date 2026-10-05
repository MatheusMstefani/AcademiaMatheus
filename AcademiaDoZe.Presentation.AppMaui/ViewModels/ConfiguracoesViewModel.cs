// Matheus Marques Stefani
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ConfiguracoesViewModel(ConfigurationHelper config) : BaseViewModel
{
    public string[] Temas { get; } = ["Sistema", "Claro", "Escuro"];
    public AppDatabaseType[] Bancos { get; } = Enum.GetValues<AppDatabaseType>();
    private string tema = Preferences.Default.Get("theme", "Sistema");
    public string Tema { get => tema; set { if (SetProperty(ref tema, value)) OnTemaChanged(value); } }
    private AppDatabaseType banco = config.Selected;
    public AppDatabaseType Banco { get => banco; set { if (SetProperty(ref banco, value)) OnBancoChanged(value); } }
    private string caminho = "";
    public string Caminho { get => caminho; set { SetProperty(ref caminho, value); } }
    private string servidor = "";
    public string Servidor { get => servidor; set { SetProperty(ref servidor, value); } }
    private string nomeBanco = "";
    public string NomeBanco { get => nomeBanco; set { SetProperty(ref nomeBanco, value); } }
    private string usuario = "";
    public string Usuario { get => usuario; set { SetProperty(ref usuario, value); } }
    private string senha = "";
    public string Senha { get => senha; set { SetProperty(ref senha, value); } }
    private string complemento = "";
    public string Complemento { get => complemento; set { SetProperty(ref complemento, value); } }
    public bool IsSqlite => Banco == AppDatabaseType.Sqlite;
    public bool IsServer => !IsSqlite;
    private void OnTemaChanged(string value)
    {
        Preferences.Default.Set("theme", value);
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage(value));
    }
    private void OnBancoChanged(AppDatabaseType value)
    {
        OnPropertyChanged(nameof(IsSqlite)); OnPropertyChanged(nameof(IsServer));
    }
    public Task LoadAsync() => RunAsync(async () =>
    {
        var settings = await config.ReadAsync(Banco);
        Caminho = settings.Path; Servidor = settings.Server; NomeBanco = settings.Database; Usuario = settings.User; Senha = settings.Password; Complemento = settings.Options;
    });
    [RelayCommand]
    private Task SalvarAsync() => RunAsync(async () =>
    {
        var settings = new DatabaseSettings(Banco, Caminho, Servidor, NomeBanco, Usuario, Senha, Complemento);
        await AppServices.TestAsync(settings);
        await config.SaveAsync(settings);
        Status = "Conexão testada e salva. As próximas operações já usarão este banco.";
    });
}
