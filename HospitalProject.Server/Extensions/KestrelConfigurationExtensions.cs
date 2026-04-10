using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace HospitalProject.Server.Extensions;

public static class KestrelConfigurationExtensions
{
    public static IWebHostBuilder ConfigureKestrelHttpsDefaults (this IWebHostBuilder webHostBuilder, IWebHostEnvironment hostEnvironment)
    {

        
        webHostBuilder.ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
               httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
               httpsOptions.HandshakeTimeout = TimeSpan.FromSeconds(10);
               
               if (hostEnvironment.IsProduction())
                {
                    httpsOptions.CheckCertificateRevocation = true;
                }

                httpsOptions.OnAuthenticate = (connectionContext, sslOptions) =>
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        sslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(new[]
                        {
                            // TLS 1.3 cipher suites
                            TlsCipherSuite.TLS_AES_256_GCM_SHA384,
                            TlsCipherSuite.TLS_AES_128_GCM_SHA256,
                            TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256,
                            
                            // TLS 1.2 cipher suites
                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
                            TlsCipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
                        }); 
                    }
                    sslOptions.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    sslOptions.EncryptionPolicy = EncryptionPolicy.RequireEncryption;
                };
            });

            serverOptions.ConfigureEndpointDefaults(endpointOptions =>
            {
                endpointOptions.Protocols = HttpProtocols.Http1AndHttp2;                
            });

            if (hostEnvironment.IsProduction())
            {
                serverOptions.Limits.MaxConcurrentConnections = 1000;
                serverOptions.Limits.MaxRequestBodySize = 10485760;
            }
        });

        return webHostBuilder;
    }
}