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
                return "Este logradouro está em uso por um aluno ou colaborador e não pode ser excluído.";
        if (ex is ArgumentException || ex is InvalidOperationException) return ex.Message;
        return "Não foi possível concluir. Confira a conexão, os campos e se o CEP já está cadastrado.";
    }
}
