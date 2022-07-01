using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tiveria.Common;

namespace Toolsfactory.Protocols.D0
{
    public class D0SerialTransport : ID0Transport
    {
        private CancellationTokenSource _CTS = new CancellationTokenSource();
        private SerialPort _port = null;
        private readonly ILogger<D0SerialTransport> _logger = null;

        public bool IsOpen => _port == null ? false : _port.IsOpen;
        public string PortName { get; private set; }
        public D0SerialTransport(ILogger<D0SerialTransport> logger, string portName)
        {
            Ensure.That(logger, nameof(logger)).IsNotNull();
            Ensure.That(portName, nameof(portName)).IsNotNullOrEmpty();
            _logger = logger;
            PortName = portName;
        }

        public void Open()
        {
            _logger.LogInformation($"Opening serial port {PortName} with 300 Baud and 7E1");
            _port = new SerialPort(PortName, 300, Parity.Even, 7, StopBits.One);
            _port.Open();
            _logger.LogInformation("Serial port successfully openened");
        }

        public void Close()
        {
            _port?.Close();
            _logger.LogInformation("Serial port closed");

        }
        public void Cancel()
        {
            _CTS.Cancel();
        }

        public async Task<byte[]> ReadStartStopAsync(byte startbyte, byte stopbyte, int timeoutms = 0)
        {
            if (_port == null || !_port.IsOpen)
                throw new InvalidOperationException("Port not initialized and opened");
            return await _port.ReadStartStopAsync(startbyte, stopbyte, _CTS.Token, timeoutms);
        }

        public async Task<byte> ReadByteAsync(int timeoutms = 0)
        {
            if (_port == null || !_port.IsOpen)
                throw new InvalidOperationException("Port not initialized and opened");
            var buffer = new byte[1];
            var cnt = await _port.ReadAsync(buffer, 0, 1, _CTS.Token, timeoutms);
            return buffer[0];
        }

        public async Task WriteBytesAsync(byte[] data)
        {
            if (_port == null || !_port.IsOpen)
                throw new InvalidOperationException("Port not initialized and opened");
            await _port.BaseStream.WriteAsync(data, 0, data.Length, _CTS.Token);
        }
    }
}
