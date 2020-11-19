using System;
using NAudio.Wave;
using NSpeex;

namespace KbAis.OperatorNetAudioClient.Utils.Codecs.Speex {
    public class SpeexDefaultDecoder : IDecoder {
        private readonly int sampleRate;
        private readonly SpeexDecoder decoder;

        public SpeexDefaultDecoder(int sampleRate, BandMode bandMode) {
            this.sampleRate = sampleRate;
            decoder = new SpeexDecoder(bandMode);
        }

        public byte[] Decode(byte[] sample, int offset, int length) {
            // Create a new buffer
            var outputBuffer = new byte[length * sampleRate];
            var outputWaveBuffer = new WaveBuffer(outputBuffer);

            // Decompress sample from speex to output buffer.
            var bytesDecoded = decoder
                .Decode(sample, offset, length, outputWaveBuffer.ShortBuffer, 0, false);

            // Copy output buffer to decompressed sample.
            var decompressedSample = new byte[bytesDecoded * 2];
            Buffer.BlockCopy(outputBuffer, 0, decompressedSample, 0, decompressedSample.Length);

            return decompressedSample;
        }
    }
}