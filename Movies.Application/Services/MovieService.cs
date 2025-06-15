using System.Reflection;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRespository _movieRespository;
    private readonly IValidator<Movie> _validator;

    public MovieService(
        IMovieRespository movieRespository,
        IValidator<Movie> validator)
    {
        _movieRespository = movieRespository;
        _validator = validator;
    }

    public async Task<bool> CreateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        return await _movieRespository.CreateAsync(movie);
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        return await _movieRespository.GetByIdAsync(id);
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        return await _movieRespository.GetBySlugAsync(slug);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _movieRespository.GetAllAsync();
    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        var movieExisting = await _movieRespository.ExistsByIdAsync(movie.Id);

        if (!movieExisting)
        {
            return null;
        }

        _ = await _movieRespository.UpdateAsync(movie);
        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        return await _movieRespository.DeleteByIdAsync(id);
    }
}