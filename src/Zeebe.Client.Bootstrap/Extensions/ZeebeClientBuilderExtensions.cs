using System;
using Zeebe.Client.Api.Builder;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Extensions
{
     public static class ZeebeClientBuilderExtensions 
     {
         public static IZeebeClient Build(this IZeebeClientBuilder builder, ClientOptions options)
         {
             return builder
                .BuildTransportBuilder(options)
                .BuildFinalStep(options)
                .BuildClient(options);
         }

        private static IZeebeClientTransportBuilder BuildTransportBuilder(this IZeebeClientBuilder builder, ClientOptions options)
        {
            if(String.IsNullOrEmpty(options.GatewayAddress))
                throw new ArgumentNullException(nameof(options.GatewayAddress));

            return builder.UseGatewayAddress(options.GatewayAddress);
        }

        private static IZeebeClientFinalBuildStep BuildFinalStep(this IZeebeClientTransportBuilder builder, ClientOptions options)
        {
            if(options.TransportEncryption == null)
                return builder.UsePlainText();

            IZeebeSecureClientBuilder clientBuilder = null;
            if (!String.IsNullOrEmpty(options.TransportEncryption.RootCertificatePath))
                clientBuilder = builder.UseTransportEncryption(options.TransportEncryption.RootCertificatePath);
            else
                clientBuilder = builder.UseTransportEncryption();

            if(!string.IsNullOrEmpty(options.TransportEncryption.AccessToken))
                return clientBuilder.UseAccessToken(options.TransportEncryption.AccessToken);
            else if(options.TransportEncryption.AccessTokenSupplier != null)
                return clientBuilder.UseAccessTokenSupplier(options.TransportEncryption.AccessTokenSupplier);

            throw new NotImplementedException($"{nameof(options.TransportEncryption)} is instantiated but none of it's properties have valid values.");
        }

        private static IZeebeClient BuildClient(this IZeebeClientFinalBuildStep builder, ClientOptions options)
        {
            if(options.KeepAlive.HasValue)
                builder = builder.UseKeepAlive(options.KeepAlive.Value);

            if(options.RetrySleepDurationProvider != null)
                builder = builder.UseRetrySleepDurationProvider(options.RetrySleepDurationProvider);

            return builder.Build();
        }
     }
}
