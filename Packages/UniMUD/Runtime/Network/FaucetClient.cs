using Grpc.Core;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Faucet;

namespace Network
{
    public class FaucetClient
    {
        public readonly string host;
        private GrpcChannel channel;
        private FaucetService.FaucetServiceClient client;

        public FaucetClient(string hostUrl)
        {
            host = hostUrl;
            var options = new GrpcChannelOptions();
            var handler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
            options.HttpHandler = handler;
            options.Credentials = ChannelCredentials.Insecure;
            channel = GrpcChannel.ForAddress(host, options);
            client = new FaucetService.FaucetServiceClient(channel);
        }

        public DripResponse DripDev(string address)
        {
            return client.DripDev(new DripDevRequest { Address = address });
        }

        public DripResponse Drip(string address)
        {
            return client.Drip(new DripRequest { Address = address });
        }

        private void OnDestroy()
        {
            channel.Dispose();
        }
    }
}
