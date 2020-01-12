using System;
using System.Threading.Tasks;

namespace Tiveria.Home.D0.Tests
{
    public class D0DataReaderDummy : ID0Transport
    {
        private byte[] _data;
        private int _position = 0;

        public void SetDummyData(byte[] data)
        {
            if (data == null || data.Length < 1)
                throw new ArgumentNullException();
            _data = data;
        }
        public Task<byte> ReadByteAsync(int timeoutms = 0)
        {
            var data = _data[_position++];
            if (_position >= _data.Length)
                _position = 0;
            return Task.FromResult(data);
        }

        public Task WriteBytesAsync(byte[] data)
        {
            return Task.CompletedTask;
        }

        public Task<byte[]> ReadStartStopAsync(byte startbyte, byte stopbyte, int timeoutms = 0)
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }
    }

}