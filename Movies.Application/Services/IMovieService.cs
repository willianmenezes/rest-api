﻿using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie);

    Task<Movie?> GetByIdAsync(Guid id, Guid? userId = null);

    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = null);

    Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options);

    Task<Movie?> UpdateAsync(Movie movie, Guid? userId = null);

    Task<bool> DeleteByIdAsync(Guid id);

    Task<int> GetTotalCountAsync(string? title, int? yearOfRelease);
}