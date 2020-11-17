using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KbAis.OperatorNetAudioClient.Utils.Networking {
    internal interface ITransmitterNode {
        Func<ReadOnlyMemory<byte>, Task> OnDataReceivedHandlerAsync { get; set; }
        void LinkNode(ITransmitterNode transmitterNode);
        Task StartAsync(CancellationToken cancellationToken);
        Task CloseAsync();
    }
}