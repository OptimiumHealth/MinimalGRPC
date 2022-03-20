namespace MinimalGRPC
{
    public interface IWeatherClient
    {
        public Task<List<WeatherForecast>> GetWeatherForecastAsync();
    }
}
