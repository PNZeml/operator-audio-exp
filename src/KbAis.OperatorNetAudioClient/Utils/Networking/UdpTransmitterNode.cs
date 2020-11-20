using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KbAis.OperatorNetAudioClient.Utils.Networking {
    internal class UdpTransmitterNode : ITransmitterNode {
        public Func<ReadOnlyMemory<byte>, Task> OnDataReceivedHandlerAsync { get; set; }
        public IPEndPoint NodeEndPoint { get; }

        private readonly int bufferSize;
        private readonly Socket socket;
        private CancellationToken cancellationToken;
        private IPEndPoint forwardingEndPoint;

        private UdpTransmitterNode(EndPoint endPoint, int bufferSize) {
            this.bufferSize = bufferSize;
            socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(endPoint);

            NodeEndPoint = (IPEndPoint)socket.LocalEndPoint;
        }

        public static UdpTransmitterNode Create(IPAddress nodeAddress, int bufferSize) {
            const int ANY_FREE_PORT = 0;

            var nodeEndPoint = new IPEndPoint(nodeAddress, ANY_FREE_PORT);

            return new UdpTransmitterNode(nodeEndPoint, bufferSize);
        }

        public void LinkNode(ITransmitterNode transmitterNode) {
            transmitterNode.OnDataReceivedHandlerAsync += DataReceivedAsync;
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            // A handler for processing received message must be linked from other transmitter.
            if (OnDataReceivedHandlerAsync == null) {
                throw new ArgumentNullException(nameof(OnDataReceivedHandlerAsync));
            }
            this.cancellationToken = cancellationToken;

            return ReceiveAsync();
        }

        public Task CloseAsync() {
            socket?.Close();
            return Task.CompletedTask;
        }

        private async Task ReceiveAsync() {
            var anyEndPoint = new IPEndPoint(IPAddress.Any, 0);

            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(bufferSize);

            try {
                while (!cancellationToken.IsCancellationRequested) {
                    var result =
                        await socket.ReceiveFromAsync(buffer, SocketFlags.None, anyEndPoint);
                    // Check if address to response is defined.
                    forwardingEndPoint ??= (IPEndPoint)result.RemoteEndPoint;
                    var message = new ReadOnlyMemory<byte>(buffer, 0, result.ReceivedBytes);
                    await OnDataReceivedHandlerAsync(message);
                }
            } catch (Exception exception) {
                Console.WriteLine(
                    $"An exception occured during receiving message in UDP node: {exception}"
                );
            } finally {
                pool.Return(buffer, true);
            }
        }

        /// <summary>
        /// Handle received message from other linked <see cref="ITransmitterNode"/>.
        /// </summary>
        private async Task DataReceivedAsync(ReadOnlyMemory<byte> message) {
            if (forwardingEndPoint == null) return;
            try {
                do {
                    var buffer = new byte[message.Length];
                    message.CopyTo(buffer);
                    // Send bytes to client.
                    var sentBytes =
                        await socket.SendToAsync(buffer, SocketFlags.None, forwardingEndPoint);
                    // If all bytes were sent return out of method.
                    if (sentBytes == message.Length) return;
                    // Slice sent bytes.
                    message = message.Slice(sentBytes);
                } while (message.Length > 0 || !cancellationToken.IsCancellationRequested);
            } catch (Exception exception) {
                Console.WriteLine(
                    $"An exception occured during message forwarding in UDP node: {exception}"
                );
            }
        }
    }
}