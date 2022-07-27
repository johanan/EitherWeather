namespace EitherWeather.Application.Models;

public record WeatherApiData(Location location, Current current)
{
    public static WeatherForecast MapToForecast(WeatherApiData data)
    {
        return new WeatherForecast()
        {
            Date = DateTime.Now,
            TemperatureC = ((int)data.current.temp_c),
            Summary = data.current.condition.text
        };
    }
};

public record Location(string localtime, string name);
public record Condition(string text);
public record Current(decimal temp_c, decimal temp_f, Condition condition);