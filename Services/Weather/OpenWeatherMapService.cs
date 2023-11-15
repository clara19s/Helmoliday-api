using HELMoliday.Contracts.Weather;
using System.Net;
using System.Text.Json.Serialization;

namespace HELMoliday.Services.Weather;
public class OpenWeatherMapService : IWeatherService
{
    private const string ApiKey = "5284104c425c1e54c5af718f79030579";
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenWeatherMapService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherResponse?> GetWeatherForCityAsync(string city)
    {
        HttpClient client = _httpClientFactory.CreateClient("weather");
        var url = $"https://pro.openweathermap.org/data/2.5/weather?q={city}&appid={ApiKey}&units=metric";

        try
        {
            var weatherResponse = await client.GetAsync(url);
            if (weatherResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var weather = await weatherResponse.Content.ReadFromJsonAsync<OpenWeatherMapWeatherResponse>();

            return new WeatherResponse(weather!.Description[0].Weather, weather!.Main.Temp, weather.Main.FeelsLike, $"https://openweathermap.org/img/wn/{ weather!.Description[0].Icon}@2x.png");
        }
        catch (HttpRequestException ex)
        {
            throw new ApplicationException("Une erreur est survenue lors de la récupération des données météorologiques.", ex);
        }
    }

    private class OpenWeatherMapWeatherResponse
    {
        [JsonPropertyName("main")] public OpenWeatherMapMain Main { get; set; }

        [JsonPropertyName("weather")] public OpenWeatherMapWeather[] Description { get; set; }

        [JsonPropertyName("visibility")] public int Visibility { get; set; }

        [JsonPropertyName("dt")] public int Dt { get; set; }

        [JsonPropertyName("timezone")] public int Timezone { get; set; }

        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("cod")] public int Cod { get; set; }
    }

    private class OpenWeatherMapWeather
    {
        [JsonPropertyName("main")] public string Weather { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("icon")] public string Icon { get; set; }
    }

    private class OpenWeatherMapMain
    {
        [JsonPropertyName("temp")] public double Temp { get; set; }

        [JsonPropertyName("feels_like")] public double FeelsLike { get; set; }

        [JsonPropertyName("temp_min")] public double TempMin { get; set; }

        [JsonPropertyName("temp_max")] public double TempMax { get; set; }

        [JsonPropertyName("pressure")] public int Pressure { get; set; }

        [JsonPropertyName("humidity")] public int Humidity { get; set; }
    }
}
