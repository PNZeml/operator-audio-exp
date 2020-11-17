using System;
using NAudio.Wave;
using NSpeex;

namespace KbAis.OperatorNetAudioClient.Utils.Codecs.Speex {
    public class SpeexDefaultEncoder : IEncoder {
        private readonly WaveBuffer inputWaveBuffer;
        private readonly SpeexEncoder encoder;

        public SpeexDefaultEncoder(int bytesPerSecond, BandMode bandMode) {
            inputWaveBuffer = new WaveBuffer(bytesPerSecond);
            encoder = new SpeexEncoder(bandMode);
        }

        public byte[] Encode(byte[] sample, int offset, int length) {
            FeedSamplesIntoEncoderInputBuffer(sample, offset, length);

            var bytesToEncode = inputWaveBuffer.ShortBufferCount;
            if (bytesToEncode % encoder.FrameSize != 0) {
                bytesToEncode -= bytesToEncode % encoder.FrameSize;
            }

            var outputBuffer = new byte[length];
            var bytesEncoded = encoder
                .Encode(inputWaveBuffer.ShortBuffer, 0, bytesToEncode, outputBuffer, 0, length);

            var encodedSample = new byte[bytesEncoded];
            Array.Copy(outputBuffer, 0, encodedSample, 0, bytesEncoded);

            ShiftLeftoverSamplesDown(bytesToEncode);

            return encodedSample;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] sample, int offset, int length) {
            Array.Copy(
                sample,
                offset,
                inputWaveBuffer.ByteBuffer,
                inputWaveBuffer.ByteBufferCount,
                length
            );
            inputWaveBuffer.ByteBufferCount += length;
        }

        private void ShiftLeftoverSamplesDown(int bytesEncoded) {
            var leftoverSamples = inputWaveBuffer.ShortBufferCount - bytesEncoded;
            Array.Copy(
                inputWaveBuffer.ByteBuffer,
                bytesEncoded * 2,
                inputWaveBuffer.ByteBuffer,
                0,
                leftoverSamples * 2
            );
            inputWaveBuffer.ShortBufferCount = leftoverSamples;
        }
    }
}