using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRespository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }


    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var trasaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            insert into movies (id, slug, title, yearofrelease)
            values (@Id, @Slug, @Title, @YearOfRelease)
            """, movie
        ));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    insert into genres (movieid, name)
                    values (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }
                ));
            }
        }

        trasaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = null)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                select m.*, ROUND(avg(r.rating), 1) as rating,
                       myr.rating as userrating
                from movies m
                left join ratings r on m.id = r.movieid
                left join ratings myr on m.id = myr.movieid
                           and myr.userid = @userId
                where id = @id
                group by id, userrating
                """, new { id, userId }
            )
        );

        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                select name from genres where movieid = @movieId
                """, new { movieId = movie.Id })
        );

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = null)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                select m.*, 
                       ROUND(avg(r.rating), 1) as rating,
                       myr.rating as userrating
                from movies m
                left join ratings r on m.id = r.movieid
                left join ratings myr on m.id = myr.movieid
                           and myr.userid = @userId
                where slug = @slug
                group by id, userrating
                """, new { slug, userId }
            )
        );

        if (movie is null)
            return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                select name from genres where movieid = @movieId
                """, new { movieId = movie.Id })
        );

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var orderClause = string.Empty;

        if (options.SortField is not null)
        {
            orderClause = $"""
                             , m.{options.SortField}
                             order by m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                           """;
        }

        var result = await connection.QueryAsync(
            new CommandDefinition(
                """
                select m.*, 
                        string_agg(distinct g.name, ',') as genres,
                        ROUND(avg(r.rating), 1) as rating,
                        myr.rating as userrating
                from movies m 
                    left join genres g on m.id = g.movieid
                    left join ratings r on m.id = r.movieid
                    left join ratings myr on m.id = myr.movieid
                                 and myr.userid = @userId
                where (@title is null or m.title like '%' || @title || '%')
                and (@yearOfRelease is null or m.yearofrelease = @yearOfRelease)
                group by m.id, userrating {orderClause}
                limit @pageSize
                offset @pageOffset
                """, new
                {
                    userId = options.UserId,
                    title = options.Title,
                    yearOfRelease = options.YearOfRelease,
                    pageSize = options.PageSize,
                    pageOffset = (options.Page - 1) * options.PageSize
                }
            ));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Rating = (float?)x.rating ?? 0,
            UserRating = x.userrating,
            Genres = Enumerable.ToList(x.genres?.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from genres
            where movieid = @movieId
            """, new { movieId = movie.Id }
        ));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                insert into genres (movieid, name)
                values (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }
            ));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            update movies
            set slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
            where id = @Id
            """, movie
        ));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from genres
            where movieid = @movieId
            """, new { movieId = id }
        ));

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from movies
            where id = @id
            """, new { id }
        ));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                select count(1) from movies
                where id = @id
                """, new { id }
            )
        );
        return exists;
    }

    public async Task<int> GetTotalCountAsync(string? title, int? yearOfRelease)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(); 
        
        return await connection.QuerySingleAsync<int> (
            new CommandDefinition(
                """
                select count(id) from movies
                where (@title is null or title like '%' || @title || '%')
                and (@yearOfRelease is null or yearofrelease = @yearOfRelease)
                """, new
                {
                    title, 
                    yearOfRelease
                }
            )
        );
    }
}