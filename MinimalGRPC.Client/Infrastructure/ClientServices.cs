using Grpc.Net.Client;
using Grpc.Net.Client.Web;

using Microsoft.Extensions.DependencyInjection;

namespace MinimalGRPC
{
    public static class ClientServices
    {
        public static void Inject(
            bool isServer,
            string baseUri,
            IServiceCollection serviceCollection)
        {
            //
            // Data access services
            //
            // Create a gRPC-Web channel pointing to the backend server
            //

            GrpcChannel channel;
            if (isServer)
            {
                var httpClientHandler = new HttpClientHandler();
#pragma warning disable CA1416 // Validate platform compatibility
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#pragma warning restore CA1416 // Validate platform compatibility

                channel = GrpcChannel.ForAddress(
                    baseUri,
                    new GrpcChannelOptions
                    {
                        HttpClient = new HttpClient(
                            new GrpcWebHandler(GrpcWebMode.GrpcWeb, httpClientHandler)),
                        MaxReceiveMessageSize = null
                    });
            }
            else
            {
                channel = GrpcChannel.ForAddress(
                    baseUri,
                    new GrpcChannelOptions
                    {
                        HttpHandler = new GrpcWebHandler(new HttpClientHandler()),
                        MaxReceiveMessageSize = null
                    });
            }
            
            serviceCollection.AddScoped<IWeatherClient, weatherClientGRPC>();
            serviceCollection.AddScoped(user =>
            {
                return new WeatherProto.WeatherProtoClient(channel);
            });
        }
    }
}
