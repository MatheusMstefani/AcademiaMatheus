// Matheus Marques Stefani
using CommunityToolkit.Mvvm.ComponentModel;
using AcademiaDoZe.Presentation.AppMaui.Services;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    private bool isBusy;
    public bool IsBusy { get => isBusy; set { SetProperty(ref isBusy, value); } }
    private string status = "";
    public string Status { get => status; set { SetProperty(ref status, value); } }
    public async Task RunAsync(Func<Task> action)
    {
        if (IsBusy) return; IsBusy = true; Status = "";
        try { await action(); } catch (Exception ex) { Status = AppServices.Error(ex); } finally { IsBusy = false; }
    }
}
