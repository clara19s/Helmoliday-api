using HELMoliday.Contracts.Weather;

namespace HELMoliday.Services.Weather;

public interface IWeatherService
{
    Task<WeatherResponse?> GetWeatherForCityAsync(string city);
}
