using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KbAis.OperatorNetAudioClient.Utils.Networking;

namespace KbAis.OperatorNetAudioClient.Features.Server {
    internal class TunnelController : IDisposable {
        private CancellationToken CancellationToken => cancellationTokenSource.Token;

        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly UdpTransmitterNode nodeA;
        private readonly UdpTransmitterNode nodeB;

        public TunnelController(
            IPEndPoint firstEndPoint,
            IPEndPoint secondEndPoint,
            int bufferSize
        ) {
            cancellationTokenSource = new CancellationTokenSource();
            nodeA = new UdpTransmitterNode(firstEndPoint, bufferSize);
            nodeB = new UdpTransmitterNode(secondEndPoint, bufferSize);

            nodeA.LinkNode(nodeB);
            nodeB.LinkNode(nodeA);

            CancellationToken.Register(async () => {
                await nodeA.CloseAsync();
                await nodeB.CloseAsync();
            });
        }

        public Task StartAsync() {
            nodeA.StartAsync(CancellationToken);
            nodeB.StartAsync(CancellationToken);

            return Task.CompletedTask;
        }

        public void Dispose() {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}