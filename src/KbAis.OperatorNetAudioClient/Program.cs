using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppFramework;
using KbAis.OperatorNetAudioClient.Features.NetAudioClient;
using KbAis.OperatorNetAudioClient.Features.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using NAudio.Wave;

namespace KbAis.OperatorNetAudioClient {
    internal class Program : ConsoleAppBase {
        public static Task Main(string[] args) =>
            Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);

        [Command("server")]
        public Task StartServerAsync(
            [Option(null)] string serverAddress,
            [Option(null)] int portA,
            [Option(null)] int portB,
            [Option(null)] int bufferSize = 4096
        ) {
            using var controller = new TunnelController(
                new IPEndPoint(IPAddress.Parse(serverAddress), portA),
                new IPEndPoint(IPAddress.Parse(serverAddress), portB),
                bufferSize
            );

            _ = controller.StartAsync();

            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();

            return Task.CompletedTask;
        }

        [Command("client-player")]
        public Task StartClientSenderAsync(
            [Option(null)] string serverAddress,
            [Option(null)] int serverPort
        ) {
            using var client =
                new UdpAudioClient(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort));
            client.StartSending();
            
            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();
            
            return Task.CompletedTask;
        }

        [Command("client-player")]
        public Task StartClientPlayerAsync(
            [Option(null)] string serverAddress,
            [Option(null)] int serverPort
        ) {
            using var cts = new CancellationTokenSource();
            using var client =
                new UdpAudioClient(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort));
            _ = client.StartPlayingAsync(cts.Token);
            
            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();

            cts.Cancel();
            
            return Task.CompletedTask;
        }
    }
}