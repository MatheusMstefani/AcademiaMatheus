// Matheus Marques Stefani
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
namespace AcademiaDoZe.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AppShell shell;
    public App(AppShell shell)
    {
        InitializeComponent(); this.shell = shell;
        ApplyTheme(Preferences.Default.Get("theme", "Sistema"));
        WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(this, (recipient, message) => ((App)recipient).ApplyTheme(message.Value));
    }
    private void ApplyTheme(string value) => UserAppTheme = value switch { "Claro" => AppTheme.Light, "Escuro" => AppTheme.Dark, _ => AppTheme.Unspecified };
    protected override Window CreateWindow(IActivationState? activationState) => new(shell)
    { Title = "Academia Matheus • Matheus Marques Stefani", Width = 1180, Height = 800, MinimumWidth = 800, MinimumHeight = 600 };
}
