// Matheus Marques Stefani
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddTransient<ILogradouroService, LogradouroService>();
        services.AddTransient<IAlunoService, AlunoService>();
        services.AddTransient<IColaboradorService, ColaboradorService>();
        services.AddTransient<IMatriculaService, MatriculaService>();

        // As fábricas criam repositórios rastreados pelo contêiner.
        // O escopo libera IDisposable ao final da operação/tela.
        services.AddTransient<ILogradouroRepository>(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return new LogradouroRepository(config.ConnectionString, config.DatabaseType);
        });
        services.AddTransient<Func<ILogradouroRepository>>(provider =>
            () => provider.GetRequiredService<ILogradouroRepository>());
        services.AddTransient<IAlunoRepository>(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return new AlunoRepository(config.ConnectionString, config.DatabaseType);
        });
        services.AddTransient<Func<IAlunoRepository>>(provider =>
            () => provider.GetRequiredService<IAlunoRepository>());
        services.AddTransient<IColaboradorRepository>(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return new ColaboradorRepository(config.ConnectionString, config.DatabaseType);
        });
        services.AddTransient<Func<IColaboradorRepository>>(provider =>
            () => provider.GetRequiredService<IColaboradorRepository>());
        services.AddTransient<IMatriculaRepository>(provider =>
        {
            var config = provider.GetRequiredService<RepositoryConfig>();
            return new MatriculaRepository(config.ConnectionString, config.DatabaseType);
        });
        services.AddTransient<Func<IMatriculaRepository>>(provider =>
            () => provider.GetRequiredService<IMatriculaRepository>());
        return services;
    }
}
