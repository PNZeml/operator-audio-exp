namespace KbAis.OperatorNetAudioClient.Utils.Codecs {
    public interface IDecoder {
        byte[] Decode(byte[] sample, int offset, int length);
    }
}