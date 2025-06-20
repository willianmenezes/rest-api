using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Auth;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.API.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    
    [Authorize]
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();

        await _movieService.CreateAsync(movie);

        var movieResponse = new
        {
            movie.Id,
            movie.Title,
            movie.YearOfRelease,
            movie.Genres
        };

        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movieResponse);
    }
    
    [Authorize]
    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var userId = HttpContext.GetUserId(); 
        
        var movie = Guid.TryParse(idOrSlug, out var parsedId)
            ? await _movieService.GetByIdAsync(parsedId, userId)
            : await _movieService.GetBySlugAsync(idOrSlug,userId);

        if (movie == null)
        {
            return NotFound();
        }

        var movieResponse = movie.MapToMovieResponse();
        return Ok(movieResponse);
    }
    
    [Authorize]
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request)
    {
        var userId = HttpContext.GetUserId(); 
        var options = request.MapToOptions()
            .WithUserId(userId);
        var movies = await _movieService.GetAllAsync(options);
        var movieCount = await _movieService.GetTotalCountAsync(options.Title, options.YearOfRelease);
        var movieResponses = movies.MapToMovieResponses(request.Page, request.PageSize, movieCount);
        return Ok(movieResponses);
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var userId = HttpContext.GetUserId(); 
        var movieToUpdate = request.MapToMovie(id);
        var updated = await _movieService.UpdateAsync(movieToUpdate, userId);
        
        if (updated == null)
        {
            return NotFound();
        }

        var response = updated.MapToMovieResponse();
        return Ok(response);
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _movieService.DeleteByIdAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}