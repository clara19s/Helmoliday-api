namespace HELMoliday.Contracts.Weather;
public record WeatherResponse(
    string Weather,
    double Temperature,
    double FeelsLike);