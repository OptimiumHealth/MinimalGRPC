#define xSSL

using Grpc.Net.Client;
using Grpc.Net.Client.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MinimalGRPC.gRPCClient;

using Optimiser.Web.Services;

using System;
using System.Net.Http;

namespace MinimalGRPC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

#if SSL
            var baseUri = "https://localhost:44324";
#else
            var baseUri = "http://localhost:45958";
#endif

            // Create a gRPC-Web channel pointing to the backend server
            var httpClientHandler = new HttpClientHandler();
#pragma warning disable CA1416 // Validate platform compatibility
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#pragma warning restore CA1416 // Validate platform compatibility
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, httpClientHandler));
            var channel = GrpcChannel.ForAddress(
                baseUri,
                new GrpcChannelOptions { HttpClient = httpClient });

            services.AddScoped<IWeatherClient, weatherClientGRPC>();
            services.AddScoped(weather =>
            {
                return new WeatherProto.WeatherProtoClient(channel);
            });

            services.AddGrpc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

#if SSL
            app.UseHttpsRedirection();
#endif

            app.UseStaticFiles();

            app.UseRouting();

            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<WeatherService>();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
