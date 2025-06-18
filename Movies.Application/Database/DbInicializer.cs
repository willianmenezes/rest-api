using Dapper;

namespace Movies.Application.Database;

public class DbInicializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInicializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync("""
                                      create table if not exists movies (
                                          id uuid primary key,
                                          slug text not null unique,
                                          title text not null,
                                          yearofrelease integer not null
                                      );
                                      """);

        await connection.ExecuteAsync("""
                                      create unique index concurrently if not exists idx_movies_slug on movies
                                      using btree (slug);
                                      """);

        await connection.ExecuteAsync("""
                                      create table if not exists genres (
                                          movieid uuid references movies(id),
                                          name text not null
                                      );
                                      """);

        await connection.ExecuteAsync("""
                                      create table if not exists ratings (
                                          movieid uuid references movies(id),
                                          userid uuid not null,
                                          rating integer not null,
                                          primary key (movieid, userid)
                                      );
                                      """);
    }
}