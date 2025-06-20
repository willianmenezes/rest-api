using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRespository
{
    Task<bool> CreateAsync(Movie movie);

    Task<Movie?> GetByIdAsync(Guid id, Guid? userId = null);

    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = null);

    Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options);

    Task<bool> UpdateAsync(Movie movie);

    Task<bool> DeleteByIdAsync(Guid id);

    Task<bool> ExistsByIdAsync(Guid id);
    
    Task<int> GetTotalCountAsync(string? title, int? yearOfRelease);
}