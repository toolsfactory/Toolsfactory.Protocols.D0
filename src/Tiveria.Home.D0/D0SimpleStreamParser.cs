using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tiveria.Common.Logging;
using Tiveria.Common.Extensions;

namespace Tiveria.Home.D0
{
    /// <summary>
    /// Simple IEC62056-21 Parser.
    /// Based on concepts in VZLogger and information found here: http://www.mayor.de/lian98/doc.de/html/g_iec62056_struct.htm
    /// </summary>
    public partial class D0SimpleStreamParser
    {
        private readonly IHexDumpLogger _hexDump;
        private readonly ID0Transport _reader;
        private readonly CancellationToken _token;
        private readonly ILogger _logger;
        private readonly bool _initialSync;

        public event EventHandler<VendorMessageEventArgs> VendorMessageEvent;
        public event EventHandler<ObisDataEventArgs> ObisDataEvent;

        public string Vendor { get; private set; } = "";
        public string Identification { get; private set; } = "";
        public int Baudrate { get; private set; } = 300;
        public char BaudrateCharacter { get; private set; } = '0';
        public bool UseShortReactionTime { get; private set; } = false;

        private Dictionary<string, ObisData> _obisItems = new Dictionary<string, ObisData>();
        public IReadOnlyDictionary<string, ObisData> ObisItems { get { return _obisItems; } }


        public D0SimpleStreamParser(ILogManager logManager, IHexDumpLogger hexDump, ID0Transport reader, CancellationToken token, bool initialSync = false)
        {
            _initialSync = initialSync;
            _hexDump = hexDump;
            _reader = reader;
            _token = token;
            _logger = logManager.GetLogger(nameof(D0SimpleStreamParser));
            InitializeFields();
        }

        private void InitializeFields()
        {
            Vendor = "";
            Identification = "";
            Baudrate = 0;
            BaudrateCharacter = (char)0x00;
            UseShortReactionTime = false;
            _obisItems.Clear();
        }
        protected virtual void RaiseVendorMessageEvent()
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            VendorMessageEvent?.Invoke(this, new VendorMessageEventArgs(Vendor, Identification, Baudrate, BaudrateCharacter, UseShortReactionTime));
        }

        protected virtual void RaiseObisDataEvent(ObisData data)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            ObisDataEvent?.Invoke(this, new ObisDataEventArgs(data.Raw, data.Code, data.Value, data.Unit));
        }

        private async Task SynchronizeAsync()
        {
            while (!_token.IsCancellationRequested)
            {
                var data = await _reader.ReadByteAsync();
                _hexDump.DumpByte(HexDumpMode.BytesIn, data);
                if (data == Constants.EndMessage)
                    break;
            }
        }

        public async Task<bool> ReadVendorMessageAsync()
        {
            var vendor = new StringBuilder();
            var identification = new StringBuilder();
            var state = ParseState.START;
            while (!_token.IsCancellationRequested && (state != ParseState.STOPPARSING))
            {
                var data = await _reader.ReadByteAsync(5000);
                _hexDump.DumpByte(HexDumpMode.BytesIn, data);

                switch (state)
                {
                    case ParseState.START:
                        if (data == Constants.StartMessage)
                            state = ParseState.VENDOR;
                        break;
                    case ParseState.VENDOR:
                        if (!Char.IsLetterOrDigit((char)data))
                        {
                            throw new MessageParsingException("Invalid character in message");
                        }
                        vendor.Append((char)data);
                        if (vendor.Length == 3)
                        {
                            state = ParseState.BAUDRATE;
                            UseShortReactionTime = Char.IsLower((char)data);
                        }
                        break;
                    case ParseState.BAUDRATE:
                        this.BaudrateCharacter = (char)data;
                        this.Baudrate = ToRealBaudrate(BaudrateCharacter);
                        state = ParseState.IDENTIFICATION;
                        break;
                    case ParseState.IDENTIFICATION:
                        if (data == Constants.CR)
                            continue;
                        if (data == Constants.LF)
                        {
                            state = ParseState.STOPPARSING;
                            break;
                        }
                        if (!((char)data).IsPrintable())
                        {
                            throw new MessageParsingException("Invalid character in message");
                        }
                        if (identification.Length < Constants.IdentificationLength)
                        {
                            identification.Append((char)data);
                        }
                        else
                        {
                            throw new MessageParsingException("Too much data for identification");
                        }
                        break;
                    default:
                        break;
                }
            }
            Vendor = vendor.ToString();
            Identification = identification.ToString();
            RaiseVendorMessageEvent();
            return true;
        }

        public async Task<bool> ReadAndParseAsync()
        {
            InitializeFields();

            if (_initialSync)
                await SynchronizeAsync();
            _hexDump.DumpBytes(HexDumpMode.BytesOut, Constants.SignOnMessage);
            await _reader.WriteBytesAsync(Constants.SignOnMessage);
            await ReadSignOnMessageAsync();
            var result = await ReadVendorMessageAsync();
            if (result)
            {
                if (UseShortReactionTime)
                    Thread.Sleep(20);
                else
                    Thread.Sleep(200);
                _hexDump.DumpBytes(HexDumpMode.BytesOut, Constants.AckMessage);
                await _reader.WriteBytesAsync(Constants.AckMessage);
                var ok = await ReadAckResponseMessageAsync();
                var result1 = await ReadObisMessageAsync();
                return result1;
            }
            return false;
        }

        private async Task<bool> ReadAckResponseMessageAsync()
        {
            var counter = 0;
            while (!_token.IsCancellationRequested && (counter < Constants.AckMessage.Length))
            {
                var data = await _reader.ReadByteAsync();
                _hexDump.DumpByte(HexDumpMode.BytesIn, data);
                if (data != Constants.AckMessage[counter++])
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ReadSignOnMessageAsync()
        {
            var counter = 0;
            while (!_token.IsCancellationRequested && (counter < Constants.SignOnMessage.Length))
            {
                var data = await _reader.ReadByteAsync();
                _hexDump.DumpByte(HexDumpMode.BytesIn, data);
                if (data != Constants.SignOnMessage[counter++])
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> ReadObisMessageAsync()
        {
            var code = new StringBuilder();
            var value = new StringBuilder();
            var unit = new StringBuilder();
            var raw = new StringBuilder();
            var obisdata = new List<ObisData>();

            var state = ParseState.OBIS_BLOCK_START;

            while (!_token.IsCancellationRequested && (state != ParseState.STOPPARSING))
            {
                var data = await _reader.ReadByteAsync();
                _hexDump.DumpByte(HexDumpMode.BytesIn, data);

                switch (state)
                {
                    case ParseState.OBIS_BLOCK_START:
                        if (data == Constants.FrameStart)
                            state = ParseState.OBIS_CODE;
                        break;
                    case ParseState.OBIS_CODE:
                        if (data == Constants.EndMessage)
                        {
                            state = ParseState.OBIS_BLOCK_END;
                            break;
                        }
                        if ((char)data == '(')
                        {
                            raw.Append((char)data);
                            state = ParseState.OBIS_VALUE;
                            break;
                        }
                        if (code.Length < Constants.ObisCodeMaxLength)
                        {
                            raw.Append((char)data);
                            code.Append((char)data);
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case ParseState.OBIS_VALUE:
                        if ((char)data == '*')
                        {
                            raw.Append((char)data);
                            state = ParseState.OBIS_UNIT;
                            break;
                        }
                        if ((char)data == ')')
                        {
                            raw.Append((char)data);
                            state = ParseState.OBIS_ITEM_END;
                            break;
                        }
                        if (value.Length < Constants.ObisValueMaxLength)
                        {
                            raw.Append((char)data);
                            value.Append((char)data);
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case ParseState.OBIS_UNIT:
                        if ((char)data == ')')
                        {
                            raw.Append((char)data);
                            state = ParseState.OBIS_ITEM_END;
                            break;
                        }
                        if (unit.Length < Constants.ObisUnitMaxLength)
                        {
                            raw.Append((char)data);
                            unit.Append((char)data);
                        }
                        else
                        {
                            throw new Exception();
                        }
                        break;
                    case ParseState.OBIS_ITEM_END:
                        if (data == Constants.CR)
                            continue;
                        if (data == Constants.LF)
                        {
                            var item = new ObisData(raw.ToString(), code.ToString(), value.ToString(), unit.ToString());
                            obisdata.Add(item);
                            RaiseObisDataEvent(item);
                            raw.Clear();
                            code.Clear();
                            value.Clear();
                            unit.Clear();
                            state = ParseState.OBIS_CODE;
                            break;
                        }
                        break;
                    case ParseState.OBIS_BLOCK_END:
                        if (data == Constants.CR || data == Constants.LF)
                            continue;
                        if (data == Constants.FrameEnd)
                        {
                            state = ParseState.STOPPARSING;
                            break;
                        }
                        throw new Exception(); // unexpected character
                    default:
                        break;
                }
            }
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

        private enum ParseState
        {
            START,
            VENDOR,
            BAUDRATE,
            IDENTIFICATION,
            OBIS_BLOCK_START,
            OBIS_ITEM_END,
            OBIS_CODE,
            OBIS_VALUE,
            OBIS_UNIT,
            OBIS_BLOCK_END,
            STOPPARSING
        }
    }
}