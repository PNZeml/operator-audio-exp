using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KbAis.OperatorNetAudioClient.Utils.Networking;

namespace KbAis.OperatorNetAudioClient.Features.Server {
    internal class TunnelController : IDisposable {
        private readonly CancellationTokenSource cts;
        private readonly ITransmitterNode nodeA;
        private readonly ITransmitterNode nodeB;

        public TunnelController(ITransmitterNode nodeA, ITransmitterNode nodeB) {
            cts = new CancellationTokenSource();
            this.nodeA = nodeA;
            this.nodeB = nodeB;

            nodeA.LinkNode(nodeB);
            nodeB.LinkNode(nodeA);

            cts.Token.Register(async () => {
                await nodeA.CloseAsync();
                await nodeB.CloseAsync();
            });
        }

        public Task StartAsync() {
            nodeA.StartAsync(cts.Token);
            nodeB.StartAsync(cts.Token);

            return Task.CompletedTask;
        }

        public void Dispose() {
            cts?.Cancel();
            cts?.Dispose();
        }
    }
}