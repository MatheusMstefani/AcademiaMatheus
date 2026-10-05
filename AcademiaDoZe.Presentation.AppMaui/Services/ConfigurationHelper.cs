// Matheus Marques Stefani
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
namespace AcademiaDoZe.Presentation.AppMaui.Services;

public sealed class ConfigurationHelper
{
    private readonly RepositoryConfig config;
    private bool loaded;
    public int Revision { get; private set; }
    public ConfigurationHelper(RepositoryConfig config)
    {
        this.config = config;
        WeakReferenceMessenger.Default.Register<BancoPreferencesUpdatedMessage>(this, (recipient, message) => ((ConfigurationHelper)recipient).Revision++);
    }
    public AppDatabaseType Selected => Enum.TryParse<AppDatabaseType>(Preferences.Default.Get("provider", "Sqlite"), out var value) ? value : AppDatabaseType.Sqlite;
    public async Task<DatabaseSettings> ReadAsync(AppDatabaseType type)
    {
        var prefix = "db." + type + ".";
        return new DatabaseSettings(type, Preferences.Default.Get(prefix + "path", Path.Combine(FileSystem.AppDataDirectory, "academia.db")),
         Preferences.Default.Get(prefix + "server", ""), Preferences.Default.Get(prefix + "database", ""), Preferences.Default.Get(prefix + "user", ""),
         type == AppDatabaseType.Sqlite ? "" : await SecureStorage.Default.GetAsync(prefix + "password") ?? "", Preferences.Default.Get(prefix + "options", ""));
    }
    public async Task EnsureLoadedAsync()
    {
        if (loaded) return;
        var candidate = (await ReadAsync(Selected)).Build();
        config.ConnectionString = candidate.ConnectionString; config.DatabaseType = candidate.DatabaseType; loaded = true;
    }
    public async Task SaveAsync(DatabaseSettings value)
    {
        var candidate = value.Build(); var prefix = "db." + value.Type + ".";
        if (value.Type != AppDatabaseType.Sqlite) await SecureStorage.Default.SetAsync(prefix + "password", value.Password);
        Preferences.Default.Set(prefix + "path", value.Path); Preferences.Default.Set(prefix + "server", value.Server);
        Preferences.Default.Set(prefix + "database", value.Database); Preferences.Default.Set(prefix + "user", value.User);
        Preferences.Default.Set(prefix + "options", value.Options); Preferences.Default.Set("provider", value.Type.ToString());
        config.ConnectionString = candidate.ConnectionString; config.DatabaseType = candidate.DatabaseType; loaded = true;
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage(value.Type.ToString()));
    }
}
