using NAudio.Wave;
using NSpeex;

namespace KbAis.OperatorNetAudioClient.Utils.Codecs.Speex {
    public class SpeexUltrawidebandCodec : ICodec {
        public WaveFormat RecordFormat =>
            recordFormat;

        private readonly WaveFormat recordFormat;
        private readonly IDecoder decoder;
        private readonly IEncoder encoder;

        public SpeexUltrawidebandCodec() {
            const int SAMPLE_RATE_KHZ = 32;
            const int BITS_PER_SAMPLE = 16;
            const int CHANNELS = 1;

            recordFormat = new WaveFormat(SAMPLE_RATE_KHZ * 1000, BITS_PER_SAMPLE, CHANNELS);
            decoder = new SpeexDefaultDecoder(SAMPLE_RATE_KHZ, BandMode.UltraWide);
            encoder =
                new SpeexDefaultEncoder(recordFormat.AverageBytesPerSecond, BandMode.UltraWide);
        }

        public byte[] Decode(byte[] sample, int offset, int length) =>
            decoder.Decode(sample, offset, length);

        public byte[] Encode(byte[] sample, int offset, int length) =>
            encoder.Encode(sample, offset, length);
    }
}