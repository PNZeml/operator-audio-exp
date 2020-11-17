using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using KbAis.OperatorNetAudioClient.Utils.Codecs;
using KbAis.OperatorNetAudioClient.Utils.Codecs.Speex;
using NAudio.Wave;

namespace KbAis.OperatorNetAudioClient.Features.NetAudioClient {
    public class UdpAudioClient : IAudioClient, IDisposable {
        private readonly UdpClient udpClient;
        private readonly ICodec codec;
        private WaveInEvent waveIn;
        private WaveOutEvent waveOut;
        private BufferedWaveProvider waveProvider;

        public UdpAudioClient(IPEndPoint serverEndPoint) {
            udpClient = new UdpClient();
            codec = new SpeexNarrowbandCodec();

            udpClient.Connect(serverEndPoint);
        }

        public void StartRecording() {
            waveIn = new WaveInEvent {
                DeviceNumber = 0, BufferMilliseconds = 50, WaveFormat = codec.RecordFormat
            };

            waveIn.DataAvailable += async (s, e) => {
                var encodedSample = codec.Encode(e.Buffer, 0, e.BytesRecorded);
                await udpClient.SendAsync(encodedSample, encodedSample.Length);
            };
            waveIn.StartRecording();
        }

        public async Task StartPlayAsync(CancellationToken cancellationToken) {
            try {
                waveOut = new WaveOutEvent();

                waveProvider = new BufferedWaveProvider(codec.RecordFormat);
                waveOut.Init(waveProvider);
            
                waveOut.Play();

                while (!cancellationToken.IsCancellationRequested) {
                    var result = await udpClient.ReceiveAsync();
                    var encodedSample = result.Buffer;
                    var decodedSample = codec.Decode(encodedSample, 0, encodedSample.Length);

                    waveProvider.AddSamples(decodedSample, 0, decodedSample.Length);
                }
            } catch(Exception exception) {
                
            }
        }

        public void Dispose() {
            udpClient?.Dispose();
            waveIn?.Dispose();
            waveOut?.Dispose();
        }
    }
}