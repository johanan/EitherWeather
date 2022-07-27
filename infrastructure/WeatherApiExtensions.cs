using System.Net;
using System.Text.Json;
using EitherWeather.Application.Models;
using LanguageExt;

namespace EitherWeather.infrastructure;

public static class WeatherApiExtensions
{
    public static Task<HttpResponseMessage> GetWeather(this HttpClient client, string apiKey, string zipCode) => 
        client.GetAsync($"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={zipCode}");

    public static async Task<Either<Exception, T>> Deserialize<T>(this HttpResponseMessage message)
    {
        try
        {
            message.EnsureSuccessStatusCode();
            var response = await JsonSerializer.DeserializeAsync<T>(await message.Content.ReadAsStreamAsync());
            return response == null 
                ? Either<Exception, T>.Left(new NullReferenceException("Response was null"))
                : Either<Exception, T>.Right(response);
        }
        catch (HttpRequestException e) when ( e.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await JsonSerializer.DeserializeAsync<ApiError>(await message.Content.ReadAsStreamAsync());
            return Either<Exception, T>.Left(new Exception(error?.error.message));
        }
        catch (Exception e)
        {
            return Either<Exception, T>.Left(e);
        }
    }

    public static EitherAsync<Exception, T> DeserializeAsync<T>(this HttpResponseMessage message)
    {
        try
        {
            message.EnsureSuccessStatusCode();
            return message.Content.ReadAsStreamAsync()
                .Bind(async stream => await JsonSerializer.DeserializeAsync<T>(stream))
                .Map(response => response == null
                    ? throw new NullReferenceException("Response was null")
                    : response);
        }
        catch (HttpRequestException e) when ( e.StatusCode == HttpStatusCode.BadRequest)
        {
            return EitherAsync<Exception, T>.LeftAsync(message.Content.ReadAsStreamAsync()
                .Bind(async stream => await JsonSerializer.DeserializeAsync<ApiError>(stream))
                .Map(error => new Exception(error?.error.message)));
        }
        catch (Exception e)
        {
            return EitherAsync<Exception, T>.Left(e);
        }
    }
}