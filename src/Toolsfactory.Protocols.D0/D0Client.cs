using System;
using System.Threading.Tasks;
using Tiveria.Common.Logging;
using Toolsfactory.Protocols.D0.Messages;

namespace Toolsfactory.Protocols.D0
{
    public class D0Client
    {
        private readonly ILogger _logger;
        private readonly ID0Transport _transport;
        private readonly IdentificationMessageParser _IDMessageParser;

        public string Vendor { get; private set; } = "";
        public string Identification { get; private set; } = "";
        public int Baudrate { get; private set; } = 300;
        public char BaudrateCharacter { get; private set; } = '0';
        public int ExpectedReactionTimeMS { get; private set; } = 200;


        public D0Client(ILogManager logManager, ID0Transport transport)
        {
            if (logManager == null)
                throw new ArgumentNullException(nameof(logManager));
            _logger = logManager.GetLogger(nameof(D0Client));
            _transport = transport;
            _IDMessageParser = new IdentificationMessageParser(logManager);
        }

        public async Task StartupAsync()
        {
            _logger.Info("Beginning Startup");
            var msg = await SendInitRequestAsync();
            await ReceiveInitResponseAsync(msg);
            await ReceiveIdentificationMessageAsync();
            _logger.Info("Startup finished");
        }

        private async Task<bool> ReceiveIdentificationMessageAsync()
        {
            _logger.Info("Receiving identification message");
            var data = await _transport.ReadStartStopAsync(Constants.StartMessage, Constants.LF);
            var ok = _IDMessageParser.Parse(data);
            if (ok)
            {
                Vendor = _IDMessageParser.Vendor;
                Identification = _IDMessageParser.Identification;
                Baudrate = _IDMessageParser.Baudrate;
                BaudrateCharacter = _IDMessageParser.BaudrateCharacter;
                _logger.Info($"Parsed identification message. Vendor: {Vendor} - Identification : {Identification}");
            } 
            else
            {
                _logger.Info("Failed parsing identification message");
            }
            return ok;
        }

        private async Task<byte[]> SendInitRequestAsync()
        {
            var msg = MessageGenerator.GenerateRequestMessage();
            _logger.Info($"Sending request message: {msg}");
            await _transport.WriteBytesAsync(msg);
            return msg;
        }

        private async Task<bool> ReceiveInitResponseAsync(byte[] initmsg)
        {
            var msg = await _transport.ReadStartStopAsync(Constants.StartMessage, Constants.LF, 5000);
            _logger.Info("Request response received: {msg}");
            return msg.Equals(initmsg);
        }
    }
}
