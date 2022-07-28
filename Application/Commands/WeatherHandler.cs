using EitherWeather.Application.Models;
using EitherWeather.infrastructure;
using LanguageExt;
using static EitherWeather.Application.Validations.ZipCodeValidation;

namespace EitherWeather.Application.Commands;

public class WeatherHandler
{
    private readonly HttpClient _client;
    private readonly string _apiKey;

    public WeatherHandler(IHttpClientFactory factory, IConfiguration configuration)
    {
        _client = factory.CreateClient();
        _apiKey = configuration.GetValue<string>("WeatherApi:Token");
    }

    public async Task<WeatherForecast> HandlePoorlyAsync(string zipCode)
    {
        var notEmpty = NotEmpty(zipCode);
        if (notEmpty.IsRight)
        {
            var notSouthBend = ValidateZipCode(zipCode);
            if (notSouthBend.IsRight)
            {
                var apiResponse = await _client.GetWeather(_apiKey, zipCode);
                var parsed = await apiResponse.Deserialize<WeatherApiData>();
                return parsed.Match(
                    Right: w => WeatherApiData.MapToForecast(w),
                    Left: e => throw e);
            }
            else notSouthBend.IfLeft(e => throw e);
        }
        else notEmpty.IfLeft(e => throw e);
        // won't compile without this. I tried really hard to make this really imperative 
        return null;
    }

    public async Task<Either<Exception, WeatherForecast>> HandleBindAsync(string zipCode)
    {
        return (await NotEmpty(zipCode)
                .Bind(notEmpty => ValidateZipCode(notEmpty))
                .BindAsync(async validZip =>
                {
                    var apiResponse = await _client.GetWeather(_apiKey, validZip);
                    return await apiResponse.Deserialize<WeatherApiData>();
                }))
            .Map(WeatherApiData.MapToForecast);
    }

    private EitherAsync<Exception, T> CastToAsync<T>(Task<T> source) => source;

    public async Task<Either<Exception, WeatherForecast>> HandleCompositionAsync(string zipCode)
    {
        Either<Exception, WeatherForecast> canDothis = await (from notEmpty in NotEmpty(zipCode).ToAsync()
                from validZip in ValidateZipCode(notEmpty).ToAsync()
                from message in CastToAsync(_client.GetWeather(_apiKey, validZip))
                from data in message.DeserializeAsync<WeatherApiData>()
                select data)
            .Map(WeatherApiData.MapToForecast);
            
        return await (from notEmpty in NotEmpty(zipCode).ToAsync()
            from validZip in ValidateZipCode(notEmpty).ToAsync()
            from message in CastToAsync(_client.GetWeather(_apiKey, validZip))
            from data in message.DeserializeAsync<WeatherApiData>().Map(WeatherApiData.MapToForecast)
            select data);
    }
    
    public async Task<Either<Exception, WeatherApiData>> HandleCompositionDifferentlyAsync(string zipCode)
    {
        return await (from notEmpty in NotEmpty(zipCode).ToAsync()
            from validZip in ValidateZipCode(notEmpty).ToAsync()
            from message in CastToAsync(_client.GetWeather(_apiKey, validZip))
            from data in message.DeserializeAsync<WeatherApiData>()
            select data);
    }
    
    public async Task<Either<Exception, WeatherApiData>> HandleProblemsAsync(string zipCode)
    {
        return await (
            from _ in IntroduceProblems(zipCode).ToAsync()
            from notEmpty in NotEmpty(zipCode).ToAsync()
            from validZip in ValidateZipCode(notEmpty).ToAsync()
            from message in CastToAsync(_client.GetWeather(_apiKey, validZip))
            from data in message.DeserializeAsync<WeatherApiData>()
            select data);
    }
}