using System.Threading.Tasks;

namespace Tiveria.Home.D0
{
    public interface ID0Transport
    {
        Task<byte[]> ReadStartStopAsync(byte startbyte, byte stopbyte, int timeoutms = 0);
        Task<byte> ReadByteAsync(int timeoutms = 0);
        Task WriteBytesAsync(byte[] data);
        void Cancel();
    }
}
