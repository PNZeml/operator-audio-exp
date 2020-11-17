using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using KbAis.OperatorNetAudioClient.Utils.Codecs;
using KbAis.OperatorNetAudioClient.Utils.Codecs.Speex;

namespace KbAis.OperatorNetAudioClient.Features.NetAudioClient {
    public class UdpAudioClient : IAudioClient {
        private readonly UdpClient udpClient;
        private readonly ICodec codec;

        public UdpAudioClient(IPEndPoint serverEndPoint) {
            udpClient = new UdpClient();
            codec = new SpeexNarrowbandCodec();

            udpClient.Connect(serverEndPoint);
        }

        public Task SendAsync(byte[] sample) =>
            udpClient.SendAsync(sample, sample.Length);
    }
}