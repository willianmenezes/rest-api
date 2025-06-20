using System.Reflection;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRespository _movieRespository;
    private readonly IValidator<Movie> _validator;
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<GetAllMoviesOptions> _validatorOptions;

    public MovieService(
        IMovieRespository movieRespository,
        IValidator<Movie> validator,
        IRatingRepository ratingRepository,
        IValidator<GetAllMoviesOptions> validatorOptions)
    {
        _movieRespository = movieRespository;
        _validator = validator;
        _ratingRepository = ratingRepository;
        _validatorOptions = validatorOptions;
    }

    public async Task<bool> CreateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        return await _movieRespository.CreateAsync(movie);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = null)
    {
        return await _movieRespository.GetByIdAsync(id, userId);
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = null)
    {
        return await _movieRespository.GetBySlugAsync(slug, userId);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options)
    {
        await _validatorOptions.ValidateAndThrowAsync(options, CancellationToken.None);

        return await _movieRespository.GetAllAsync(options);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = null)
    {
        await _validator.ValidateAndThrowAsync(movie);

        var movieExisting = await _movieRespository.ExistsByIdAsync(movie.Id);

        if (!movieExisting)
        {
            return null;
        }

        _ = await _movieRespository.UpdateAsync(movie);

        if (!userId.HasValue)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id);
            movie.Rating = rating ?? 0;
            return movie;
        }

        var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value);
        movie.UserRating = ratings.UserRating ?? 0;
        movie.Rating = ratings.Rating ?? 0;

        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        return await _movieRespository.DeleteByIdAsync(id);
    }

    public async Task<int> GetTotalCountAsync(string? title, int? yearOfRelease)
    {
        return await _movieRespository.GetTotalCountAsync(title, yearOfRelease);
    }
}