using Grpc.Core;

using MinimalGRPC.gRPCClient;

using System;
using System.Threading.Tasks;

//
//  2021-06-14  Mark Stega
//              Created
//

namespace MinimalGRPC
{
    public class WeatherService : WeatherProto.WeatherProtoBase
    {
        #region GetWeatherForecast

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public override Task<WeatherForecastReply> GetWeatherForecast(WeatherForecastRequest request, ServerCallContext context)
        {
            var rng = new Random();
            var startDate = DateTime.Now;
            var reply = new WeatherForecastReply { };

            for (int i = 0; i < 5; i++)
            {
                var weatherDay = new WeatherDay();
                weatherDay.Date = (startDate.AddDays(i)).ToShortDateString();
                weatherDay.TemperatureC = rng.Next(-20, 55);
                weatherDay.Summary = Summaries[rng.Next(Summaries.Length)];
                reply.WeatherList.Add(weatherDay);
            }
            return Task.FromResult(reply);
        }

        #endregion
    }
}
