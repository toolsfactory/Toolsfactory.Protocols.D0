using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Tiveria.Common.Builders;

namespace Toolsfactory.Protocols.D0
{
    public static class SerialPortExtensions
    {
        public static async Task<int> ReadUntilAsync(this SerialPort port, byte chr, byte[] buffer, CancellationToken token = default, int timeoutms = 0)
        {
            var internalToken = GenerateTokenWithTimeout(token, timeoutms);
            int totalReceived = 0;
            var maxLen = buffer.Length;
            while (!token.IsCancellationRequested)
            {
                totalReceived += await port.BaseStream.ReadAsync(buffer, totalReceived, 1, internalToken);
                if (buffer[totalReceived - 1] == chr || (totalReceived >= maxLen))
                    break;
            }
            return totalReceived;
        }

        public static async Task<byte[]> ReadStartStopAsync(this SerialPort port, byte startbyte, byte stopbyte, CancellationToken token = default, int timeoutms = 0)
        {
            var internalToken = GenerateTokenWithTimeout(token, timeoutms);
            var buffer = new ByteArrayBuilder();
            bool startByteFound = false;
            byte[] tempBuffer = new byte[1];
            while (!token.IsCancellationRequested)
            {
                //number of total received data bytes
                int received = await port.BaseStream.ReadAsync(tempBuffer, 0, 1, internalToken);
                if (!startByteFound)
                {
                    if (tempBuffer[0] == startbyte)
                    {
                        startByteFound = true;
                        buffer.Append(startbyte);
                    }
                    continue;
                }
                else
                {
                    buffer.Append(tempBuffer[0]);
                    if (tempBuffer[0] == stopbyte)
                    {
                        break;
                    }
                }
            }
            return buffer.ToArray();
        }

        public static Task<int> ReadAsync(this SerialPort port, byte[] array, int offset, int count, CancellationToken cancellationToken = default, int timeoutms = 0)
        {
            CancellationToken internalToken = GenerateTokenWithTimeout(cancellationToken, timeoutms); ;
            return port.BaseStream.ReadAsync(array, offset, count, internalToken);
        }

        private static CancellationToken GenerateTokenWithTimeout(CancellationToken cancellationToken, int timeoutms)
        {
            CancellationToken internalToken = cancellationToken;
            if (timeoutms > 0)
            {
                var timeoutCTS = new CancellationTokenSource(timeoutms);
                var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(timeoutCTS.Token, cancellationToken);
                internalToken = linkedCTS.Token;
            }
            return internalToken;
        }
    }
}
