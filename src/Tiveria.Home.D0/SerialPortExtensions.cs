using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Tiveria.Common.Builders;

namespace Tiveria.Home.D0
{
    public static class SerialPortExtensions
    { 
        public static async Task<int> ReadUntilAsync(this SerialPort port, byte chr, byte[] buffer, CancellationToken token = default, int timeoutms = 0)
        {
            CancellationTokenSource linkedCTS = null;
            CancellationTokenSource timeoutCTS = null;
            try
            {
                CancellationToken internalToken = token;
                timeoutCTS = new CancellationTokenSource(timeoutms);
                if (timeoutms > 0)
                {
                    linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(timeoutCTS.Token, token);
                    internalToken = linkedCTS.Token;
                }
                //number of total received data bytes
                int totalReceived = 0;
                var maxLen = buffer.Length;
                while (!token.IsCancellationRequested)
                {
                    totalReceived += await port.BaseStream.ReadAsync(buffer, totalReceived, 1, internalToken);
                    if (buffer[totalReceived - 1] == chr || (totalReceived >= maxLen))
                    {
                        break;
                    }
                }
                return totalReceived;
            }
            finally
            {
                linkedCTS?.Dispose();
                timeoutCTS?.Dispose();
            }
        }

        public static async Task<byte[]> ReadStartStopAsync(this SerialPort port, byte startbyte, byte stopbyte, CancellationToken token = default, int timeoutms = 0)
        {
            CancellationTokenSource linkedCTS = null;
            CancellationTokenSource timeoutCTS = null;
            try
            {
                CancellationToken internalToken = token;
                timeoutCTS = new CancellationTokenSource(timeoutms);
                if (timeoutms > 0)
                {
                    linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(timeoutCTS.Token, token);
                    internalToken = linkedCTS.Token;
                }
                //number of total received data bytes
                int received = 0;
                var buffer = new ByteArrayBuilder();
                bool startByteFound = false;
                byte[] tempBuffer = new byte[1];
                while (!token.IsCancellationRequested)
                {
                    received = await port.BaseStream.ReadAsync(tempBuffer, 0, 1, internalToken);
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
            finally
            {
                linkedCTS?.Dispose();
                timeoutCTS?.Dispose();
            }
        }
    }
}
