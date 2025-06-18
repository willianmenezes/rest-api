using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var sql = """
                  insert into ratings (movieid, userid, rating)
                  values (@MovieId, @UserId, @Rating)
                  on conflict (movieid, userid) do update
                  set rating = @Rating
                  """;

        var result = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { MovieId = movieId, UserId = userId, Rating = rating }));

        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var sql = "select round(AVG(rating), 1) FROM ratings WHERE movieid = @MovieId";
        return await connection.ExecuteScalarAsync<float?>(sql, new { MovieId = movieId });
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var sql = """
                  select round(avg(r.rating), 1),
                      (select rating from ratings where movieid = @MovieId and userid = @UserId limit 1)
                  from ratings r
                  where r.movieid = @MovieId
                  """;
        return await connection.QuerySingleOrDefaultAsync<(float? Rating, int? UserRating)>(sql,
            new { MovieId = movieId, UserId = userId });
    }
}