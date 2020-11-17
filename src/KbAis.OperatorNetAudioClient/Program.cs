using System;
using System.Net;
using System.Threading.Tasks;
using ConsoleAppFramework;
using KbAis.OperatorNetAudioClient.Features.Server;
using Microsoft.Extensions.Hosting;
using NAudio.Wave;

namespace KbAis.OperatorNetAudioClient {
    internal class Program : ConsoleAppBase {
        public static Task Main(string[] args) =>
            Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);

        [Command("server")]
        public async Task StartServerAsync(
            [Option("a")] int portA,
            [Option("b")] int portB,
            [Option("s")] int bufferSize = 1024
        ) {
            var controller = new TunnelController(
                new IPEndPoint(IPAddress.Loopback, portA),
                new IPEndPoint(IPAddress.Loopback, portB),
                bufferSize
            );

            await controller.StartAsync();

            Console.WriteLine("Press any key to shutdown...");
            Console.ReadKey();

            controller.Dispose();
        }

        [Command("client")]
        public async Task StartClientAsync() {
            var foo = WaveIn.DeviceCount;
            Console.Write(foo);
        }
    }
}