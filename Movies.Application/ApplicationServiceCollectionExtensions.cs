using System.Diagnostics.Contracts;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRespository, MovieRepository>();
        services.AddSingleton<IMovieService, MovieService>();
        services.AddSingleton<IRatingService, RatingService>();
        services.AddSingleton<IRatingRepository, RatingRepository>();
        services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
        return services;
    }


    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory, NpgSqlConnectionFactory>(_ =>
            new NpgSqlConnectionFactory(connectionString));

        services.AddSingleton<DbInicializer>();
        return services;
    }
}