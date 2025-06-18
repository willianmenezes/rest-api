namespace Movies.Application.Repositories;

public interface IRatingRepository
{
    Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating);

    Task<float?> GetRatingAsync(Guid movieId);

    Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId);
}