using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppFramework;
using KbAis.OperatorNetAudioClient.Features.Client;
using KbAis.OperatorNetAudioClient.Features.Server;
using KbAis.OperatorNetAudioClient.Utils.Networking;
using Microsoft.Extensions.Hosting;

namespace KbAis.OperatorNetAudioClient {
    internal class Program : ConsoleAppBase {
        public static Task Main(string[] args) =>
            Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);

        [Command("server")]
        public Task StartServerAsync(
            [Option("a")] string serverAddress,
            [Option("b")] int bufferSize = 4096
        ) {
            var serverIpAddress = IPAddress.Parse(serverAddress);
            var nodeA = UdpTransmitterNode.Create(serverIpAddress, bufferSize);
            var nodeB = UdpTransmitterNode.Create(serverIpAddress, bufferSize);

            using var controller = new TunnelController(nodeA, nodeB);

            _ = controller.StartAsync();

            Console.WriteLine($"Node A bound to {nodeA.NodeEndPoint.Port}");
            Console.WriteLine($"Node B bound to {nodeB.NodeEndPoint.Port}");

            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();

            return Task.CompletedTask;
        }

        [Command("client-sender")]
        public async Task StartClientSenderAsync(
            [Option("a")] string serverAddress,
            [Option("p")] int serverPort
        ) {
            using var client = new UdpAudioClient();
            await client.LogInAsync(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort));
            client.StartSending();

            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();
        }

        [Command("client-player")]
        public async Task StartClientPlayerAsync(
            [Option("a")] string serverAddress,
            [Option("p")] int serverPort
        ) {
            using var cts = new CancellationTokenSource();
            using var client = new UdpAudioClient();
            await client.LogInAsync(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort));
            _ = client.StartPlayingAsync(cts.Token);

            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();

            cts.Cancel();
        }
    }
}