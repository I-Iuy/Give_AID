using System.Net.Http;
using Google.Apis.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Be.Services
{
    public class NoSslValidationHttpClientFactory : Google.Apis.Http.IHttpClientFactory
    {
        public ConfigurableHttpClient CreateHttpClient(CreateHttpClientArgs args)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            return new ConfigurableHttpClient(new ConfigurableMessageHandler(handler)
            {
                ApplicationName = args.ApplicationName
            });
        }
    }
}
