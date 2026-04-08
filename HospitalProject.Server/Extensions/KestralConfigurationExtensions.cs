using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace HospitalProject.Server.Extensions;

public static class KestrelConfigurationExtensions
{
    public static IWebHostBuilder ConfigureKestrelHttpsDefaults (this IWebHostBuilder webHostBuilder, IWebHostEnvironment hostEnvironment)
    {

        
        webHostBuilder.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
               httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

            });

            options.ConfigureEndpointDefaults(endpointOptions =>
            {
                endpointOptions.Protocols = HttpProtocols.Http1AndHttp2;

                endpointOptions.UseHttps();
            });
        });

        return webHostBuilder;
    }
}