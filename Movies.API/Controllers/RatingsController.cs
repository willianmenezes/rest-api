using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Auth;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.API.Controllers;

[ApiController]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    public async Task<IActionResult> RateMovie([FromRoute] Guid id, [FromBody] RateMovieRequest request)
    {
        var userId = HttpContext.GetUserId();

        var result = await _ratingService.RateMovieAsync(id, userId!.Value, request.Rating);

        return result
            ? Ok()
            : NotFound(new { Message = "Movie not found or rating is invalid." });
    }
}