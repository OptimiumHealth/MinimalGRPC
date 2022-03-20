using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using MinimalGRPC;

namespace MinimalGRPC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webApplicationBuilder =>
                {
                    webApplicationBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
