using NAudio.Wave;

namespace KbAis.OperatorNetAudioClient.Utils.Codecs {
    public interface ICodec : IDecoder, IEncoder {
        WaveFormat RecordFormat { get; }
    }
}