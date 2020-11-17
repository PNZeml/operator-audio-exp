namespace KbAis.OperatorNetAudioClient.Utils.Codecs {
    public interface IEncoder {
        byte[] Encode(byte[] sample, int offset, int length);
    }
}