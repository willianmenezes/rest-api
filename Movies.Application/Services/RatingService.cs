using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMovieRespository _movieRespository;

    public RatingService(IRatingRepository ratingRepository, IMovieRespository movieRespository)
    {
        _ratingRepository = ratingRepository;
        _movieRespository = movieRespository;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating)
    {
        if (rating is <= 0 or > 5)
        {
            throw new ValidationException([
                new ValidationFailure()
                {
                    PropertyName = "Rating",
                    ErrorMessage = "Rating must be between 1 and 5."
                }
            ]);
        }

        var movieExists = await _movieRespository.ExistsByIdAsync(movieId);

        if (!movieExists)
        {
            return false;
        }

        return await _ratingRepository.RateMovieAsync(movieId, userId, rating);
    }
}