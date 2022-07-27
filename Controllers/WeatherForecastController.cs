using EitherWeather.Application.Commands;
using EitherWeather.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace EitherWeather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("WeatherPoorly/{zipCode}")]
    public async Task<IActionResult> GetWeatherPoorly([FromRoute] string zipCode, [FromServices] WeatherHandler handler)
    {
        try
        {
            return new OkObjectResult(await handler.HandlePoorlyAsync(zipCode));
        }
        catch (Exception e)
        {
            var details = new ProblemDetails();
            details.Extensions.Add("errors", new string[] { e.Message });
            return new BadRequestObjectResult(details);
        }
    }
    
    [HttpGet("WeatherBind/{zipCode}")]
    public async Task<IActionResult> GetWeatherBind([FromRoute] string zipCode, [FromServices] WeatherHandler handler)
    {
        try
        {
            return new OkObjectResult(await handler.HandleBindAsync(zipCode));
        }
        catch (Exception e)
        {
            var details = new ProblemDetails();
            details.Extensions.Add("errors", new string[] { e.Message });
            return new BadRequestObjectResult(details);
        }
    }
    
    [HttpGet("WeatherComp/{zipCode}")]
    public async Task<IActionResult> GetWeatherComp([FromRoute] string zipCode, [FromServices] WeatherHandler handler)
    {
        return (await handler.HandleCompositionAsync(zipCode)).ToActionResult();
    }
    
    [HttpGet("WeatherCompDifferently/{zipCode}")]
    public async Task<IActionResult> GetWeatherCompDifferently([FromRoute] string zipCode, [FromServices] WeatherHandler handler)
    {
        // can compose however you want
        return (await handler.HandleCompositionDifferentlyAsync(zipCode))
            .Map(WeatherApiData.MapToForecast)
            .ToActionResult();
    }
    
    [HttpGet("WeatherProblems/{zipCode}")]
    public async Task<IActionResult> GetWeatherProblems([FromRoute] string zipCode, [FromServices] WeatherHandler handler)
    {
        return (await handler.HandleProblemsAsync(zipCode))
            .Map(WeatherApiData.MapToForecast)
            .ToActionResult();
    }
}
