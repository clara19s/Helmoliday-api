using HELMoliday.Services.Weather;
using Microsoft.AspNetCore.Mvc;

namespace HELMoliday.Controllers;

[Route("weather")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather([FromQuery] string q)
    {
        var weather = await _weatherService.GetWeatherForCityAsync(q);
        return weather is null ? NotFound() : Ok(weather);
    }
}
