using System;
using System.Text;
using Tiveria.Common.Logging;
using Tiveria.Common.Extensions;

namespace Tiveria.Home.D0
{
    internal enum VendorMessageParseState
    {
        START,
        VENDOR,
        BAUDRATE,
        IDENTIFICATION,
        STOP
    }
    public class IdentificationMessageParser
    {
        private readonly ILogger _logger;

        public string Vendor { get; private set; } = "";
        public string Identification { get; private set; } = "";
        public int Baudrate { get; private set; } = 300;
        public char BaudrateCharacter { get; private set; } = '0';
        public int ExpectedReactionTimeMS { get; private set; } = 200;

        public IdentificationMessageParser(ILogManager logManager)
        {
            if (logManager == null)
                throw new ArgumentNullException(nameof(logManager));
            this._logger = logManager.GetLogger(nameof(IdentificationMessageParser));
        }

        public bool Parse(byte[] input)
        {
            var vendor = new StringBuilder();
            var identification = new StringBuilder();
            var state = VendorMessageParseState.START;
            var pos = 0;
            while ((pos < input.Length) && (state != VendorMessageParseState.STOP))
            {
                var data = input[pos++];

                switch (state)
                {
                    case VendorMessageParseState.START:
                        if (data == Constants.StartMessage)
                            state = VendorMessageParseState.VENDOR;
                        break;
                    case VendorMessageParseState.VENDOR:
                        if (!Char.IsLetterOrDigit((char)data))
                        {
                            throw new Exception("Invalid character in message");
                        }
                        vendor.Append((char)data);
                        if (vendor.Length == 3)
                        {
                            state = VendorMessageParseState.BAUDRATE;
                            if (Char.IsUpper((char)data))
                                this.ExpectedReactionTimeMS = 200;
                            else
                                this.ExpectedReactionTimeMS = 20;
                        }
                        break;
                    case VendorMessageParseState.BAUDRATE:
                        this.BaudrateCharacter = (char)data;
                        this.Baudrate = ToRealBaudrate(BaudrateCharacter);
                        state = VendorMessageParseState.IDENTIFICATION;
                        break;
                    case VendorMessageParseState.IDENTIFICATION:
                        if (data == Constants.CR)
                            continue;
                        if (data == Constants.LF)
                        {
                            state = VendorMessageParseState.STOP;
                            break;
                        }
                        if (!((char)data).IsPrintable())
                        {
                            throw new Exception("Invalid character in message");
                        }
                        if (identification.Length < Constants.IdentificationLength)
                        {
                            identification.Append((char)data);
                        }
                        else
                        {
                            throw new Exception("Too much data for identification");
                        }
                        break;
                    default:
                        break;
                }
            }
            this.Vendor = vendor.ToString();
            this.Identification = identification.ToString();
            return true;
        }

        private static int ToRealBaudrate(char rateid) =>
            rateid switch
            {
                '0' => 300,
                '1' => 600,
                '2' => 1200,
                '3' => 2400,
                '4' => 4800,
                '5' => 9600,
                '6' => 19200,
                _ => 300
            };
    }
}
