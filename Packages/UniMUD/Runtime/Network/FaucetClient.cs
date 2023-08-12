using System;
using Grpc.Core;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using Faucet;

namespace Network
{
    public class FaucetClient: IDisposable
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

        public async UniTask<DripResponse> Drip(string address)
        {
            try
            {
                return await client.DripDevAsync(new DripDevRequest { Address = address });
            }
            catch (RpcException e)
            {
                throw e;
            }
        }

        public void Dispose()
        {
            channel.Dispose();
        }
    }
}
