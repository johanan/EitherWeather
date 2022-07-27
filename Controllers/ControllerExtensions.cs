using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace EitherWeather.Controllers;

public static class ControllerExtensions
{
    public static IActionResult ToActionResult<T>(this Either<Exception, T> result)
    {
        return result
            .Match<IActionResult>(
                Right: r => new OkObjectResult(r),
                Left: e =>
                {
                    var details = new ProblemDetails();
                    details.Extensions.Add("errors", new[] { e.Message });
                    return new BadRequestObjectResult(details);
                });
    }
}