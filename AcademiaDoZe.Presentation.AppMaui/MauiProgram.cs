// Matheus Marques Stefani
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Services;
using Microsoft.Extensions.DependencyInjection;
namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<App>();
        builder.Services.AddSingleton(new DatabaseSettings(AppDatabaseType.Sqlite, Path.Combine(FileSystem.AppDataDirectory, "academia.db")).Build()).AddApplicationServices();
        builder.Services.AddSingleton<ConfigurationHelper>().AddSingleton<AppServices>().AddSingleton<AppShell>();
        return builder.Build();
    }
}
