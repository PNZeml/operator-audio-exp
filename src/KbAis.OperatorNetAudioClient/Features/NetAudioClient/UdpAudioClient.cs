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

        public UdpAudioClient(IPEndPoint serverEndPoint) {
            udpClient = new UdpClient();
            codec = new SpeexNarrowbandCodec();

            udpClient.Connect(serverEndPoint);
            udpClient.Send(new byte[] { 0x00 }, 1);
        }

        public void StartSending() {
            using var waveIn = new WaveInEvent {
                DeviceNumber = 0,
                BufferMilliseconds = 100,
                WaveFormat = codec.RecordFormat
            };
            waveIn.DataAvailable += (s, e) => {
                var encodedSample = codec.Encode(e.Buffer, 0, e.BytesRecorded);
                udpClient.Send(encodedSample, encodedSample.Length);
            };
            waveIn.StartRecording();
        }

        public async Task StartPlayingAsync(CancellationToken cancellationToken) {
            try {
                using var waveOut = new WasapiOut();
                var waveProvider = new BufferedWaveProvider(codec.RecordFormat) {
                    DiscardOnBufferOverflow = true
                };
                waveOut.Init(waveProvider);
                var durationPlayTimeSpan = TimeSpan.FromMilliseconds(100);
                
                while (!cancellationToken.IsCancellationRequested) {
                    var receiveResult = await udpClient.ReceiveAsync();
                    var decodedSample =
                        codec.Decode(receiveResult.Buffer, 0, receiveResult.Buffer.Length);
                    waveProvider.AddSamples(decodedSample, 0, decodedSample.Length);

                    if (waveProvider.BufferedDuration >= durationPlayTimeSpan) {
                        waveOut.Play();
                    } else {
                        waveOut.Stop();
                    }
                }
            } catch(Exception exception) {
                Console.WriteLine(exception);
            }
        }

        public void Dispose() =>
            udpClient?.Close();
    }
}