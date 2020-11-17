﻿using NAudio.Wave;
using NSpeex;

namespace KbAis.OperatorNetAudioClient.Utils.Codecs.Speex {
    public class SpeexNarrowbandCodec : ICodec {
        public WaveFormat RecordFormat => recordFormat;

        private readonly WaveFormat recordFormat;
        private readonly IDecoder decoder;
        private readonly IEncoder encoder;

        public SpeexNarrowbandCodec() {
            const int SAMPLE_RATE_KHZ = 8;
            const int BITS_PER_SAMPLE = 16;
            const int CHANNELS = 1;

            recordFormat = new WaveFormat(SAMPLE_RATE_KHZ, BITS_PER_SAMPLE, CHANNELS);
            decoder = new SpeexDefaultDecoder(SAMPLE_RATE_KHZ, BandMode.Narrow);
            encoder = new SpeexDefaultEncoder(recordFormat.AverageBytesPerSecond, BandMode.Narrow);
        }

        public byte[] Decode(byte[] sample, int offset, int length) =>
            decoder.Decode(sample, offset, length);

        public byte[] Encode(byte[] sample, int offset, int length) =>
            encoder.Encode(sample, offset, length);
    }
}