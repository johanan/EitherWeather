namespace EitherWeather.Application.Models;

public record ApiError(ErrorData error);
public record ErrorData(int code, string message);