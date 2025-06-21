using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Auth;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.API.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
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
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
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

        return CreatedAtAction(nameof(GetV1), new { idOrSlug = movie.Id }, movieResponse);
    }

    [MapToApiVersion(2.0)]
    [Authorize]
    [HttpGet(ApiEndpoints.Movies.Get)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetV1(
        [FromServices] LinkGenerator linkGenerator,
        [FromRoute] string idOrSlug)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var parsedId)
            ? await _movieService.GetByIdAsync(parsedId, userId)
            : await _movieService.GetBySlugAsync(idOrSlug, userId);

        if (movie == null)
        {
            return NotFound();
        }

        var movieResponse = movie.MapToMovieResponse();

        var movieObj = new { id = movie.Id };

        movieResponse.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV1), values: new { idOrSlug }),
            Rel = "self",
            Type = "GET"
        });

        movieResponse.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: new { id = idOrSlug }),
            Rel = "self",
            Type = "PUT"
        });

        movieResponse.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: new { id = idOrSlug }),
            Rel = "self",
            Type = "DELETE"
        });

        return Ok(movieResponse);
    }

    [MapToApiVersion(2.0)]
    [Authorize]
    [HttpGet(ApiEndpoints.Movies.Get)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetV2(
        [FromServices] LinkGenerator linkGenerator,
        [FromRoute] string idOrSlug)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var parsedId)
            ? await _movieService.GetByIdAsync(parsedId, userId)
            : await _movieService.GetBySlugAsync(idOrSlug, userId);

        if (movie == null)
        {
            return NotFound();
        }

        var movieResponse = movie.MapToMovieResponse();

        var movieObj = new { id = movie.Id };

        movieResponse.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV1), values: new { idOrSlug }),
            Rel = "self",
            Type = "GET"
        });

        movieResponse.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: new { id = idOrSlug }),
            Rel = "self",
            Type = "PUT"
        });

        movieResponse.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: new { id = idOrSlug }),
            Rel = "self",
            Type = "DELETE"
        });

        return Ok(movieResponse);
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
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