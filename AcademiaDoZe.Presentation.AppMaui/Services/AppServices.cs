// Matheus Marques Stefani
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace AcademiaDoZe.Presentation.AppMaui.Services;

public sealed class AppServices(IServiceScopeFactory scopes, ConfigurationHelper config)
{
    public async Task<T> UseAsync<TService, T>(Func<TService, CancellationToken, Task<T>> action) where TService : notnull
    {
        await config.EnsureLoadedAsync();
        using var scope = scopes.CreateScope(); using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        return await action(scope.ServiceProvider.GetRequiredService<TService>(), timeout.Token);
    }
    public static async Task TestAsync(DatabaseSettings settings)
    {
        using var provider = new ServiceCollection().AddSingleton(settings.Build()).AddApplicationServices().BuildServiceProvider();
        using var scope = provider.CreateScope(); using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await scope.ServiceProvider.GetRequiredService<ILogradouroService>().ObterTodosAsync(timeout.Token);
    }
    public static string Error(Exception ex)
    {
        if (ex is OperationCanceledException) return "O banco demorou para responder. Confira as configurações e tente novamente.";
        for (Exception? current = ex; current is not null; current = current.InnerException)
            if (current.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) || current.Message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase))
                return "Este cadastro possui registros vinculados e não pode ser excluído. Confira os vínculos antes de tentar novamente.";
        if (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException) return ex.Message;
        return "Não foi possível concluir. Confira a conexão e os campos; CPF, e-mail ou CEP duplicados também podem impedir a gravação.";
    }
}
