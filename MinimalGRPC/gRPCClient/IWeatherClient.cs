using MinimalGRPC.Data;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinimalGRPC.gRPCClient
{
    public interface IWeatherClient
    {
        public Task<List<WeatherForecast>> GetWeatherForecastAsync();
    }
}
