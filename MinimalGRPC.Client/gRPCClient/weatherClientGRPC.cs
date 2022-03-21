using MinimalGRPC;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//
//  2021-06-14  Mark Stega
//

namespace MinimalGRPC
{
    public class weatherClientGRPC : IWeatherClient
    {
        #region members
        private WeatherProto.WeatherProtoClient pWeatherProtoClient { get; set; }
        #endregion

        #region ctor
        private weatherClientGRPC() { }
        public weatherClientGRPC(
            WeatherProto.WeatherProtoClient protoClient)
        {
            pWeatherProtoClient = protoClient;
        }
        #endregion

        #region GetWeatherForecastAsync
        public async Task<List<WeatherForecast>> GetWeatherForecastAsync()
        {
            var request = new WeatherForecastRequest { };
            var reply = await pWeatherProtoClient.GetWeatherForecastAsync(request);

            var res = new List<WeatherForecast>();

            foreach (var wd in reply.WeatherList)
            {
                var f = new WeatherForecast();
                f.Date = Convert.ToDateTime(wd.Date);
                f.TemperatureC = wd.TemperatureC;
                f.Summary = wd.Summary;

                res.Add(f);
            }

            return res;
        }

        #endregion

    }
}
