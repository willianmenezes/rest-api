using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptableSortFields =
    {
        nameof(GetAllMoviesOptions.Title).ToLower(),
        nameof(GetAllMoviesOptions.YearOfRelease).ToLower()
    };

    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
        
        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by Title or YearOfRelease.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);
    }
}