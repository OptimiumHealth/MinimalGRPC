#define SSL
#define xnet31

#if net31
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MinimalGRPC.gRPCClient;

using Optimiser.Web.Services;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
#else
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MinimalGRPC.gRPCClient;

using Optimiser.Web.Services;

using System.Net.Http;
#endif

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
            var baseUri = "https://localhost:44319";
#else
            var baseUri = "http://localhost:45958";

#if net31
            // This switch must be set before creating the GrpcChannel/HttpClient.
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
#endif
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
            services.AddGrpcReflection();

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

#if net31
            app.Use((c, next) =>
            {
                if (c.Request.ContentType == "application/grpc")
                {
                    var current = c.Features.Get<IHttpResponseFeature>();
                    c.Features.Set<IHttpResponseFeature>(new HttpSysWorkaroundHttpResponseFeature(current));
                }
                return next();
            });
#endif

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<WeatherService>();
                endpoints.MapGrpcReflectionService();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

#if net31
    public class HttpSysWorkaroundHttpResponseFeature : IHttpResponseFeature
    {
        private readonly IHttpResponseFeature _inner;

        public HttpSysWorkaroundHttpResponseFeature(IHttpResponseFeature inner)
        {
            _inner = inner;
            _inner.OnStarting(o =>
            {
                HasStarted = true;
                return Task.CompletedTask;
            }, null);
        }

        [Obsolete]
        public Stream Body
        {
            get => _inner.Body;
            set { _inner.Body = value; }
        }
        public bool HasStarted { get; private set; }
        public IHeaderDictionary Headers
        {
            get => _inner.Headers;
            set { _inner.Headers = value; }
        }
        public string ReasonPhrase
        {
            get => _inner.ReasonPhrase;
            set { _inner.ReasonPhrase = value; }
        }
        public int StatusCode
        {
            get => _inner.StatusCode;
            set { _inner.StatusCode = value; }
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
            _inner.OnCompleted(callback, state);
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            _inner.OnStarting(callback, state);
        }
    }
#endif
}
