namespace Tiveria.Home.D0
{
    public static class Constants
    {
        public const byte FrameStart = 0x02;        // STX
        public const byte FrameEnd   = 0x03;        // ETX
        public const byte StartOfHeader = 0x01;     // SOH
        public const byte EndOfText = 0x04;         // EOT
        public const byte StartMessage = 0x2F;      // "/"
        public const byte EndMessage = 0x21;        // "!"
        public const byte CR = 0x0D;                // Cariage Return
        public const byte LF = 0x0A;                // Line Feed
        public const byte Request = 0x3F;           // "?"
        public const byte Ack = 0x06;               // Acknowledge
        public const byte NAck = 0x15;              // Negative Acknowledge

        public static readonly byte[] SignOnMessage = new byte[5] { StartMessage, Request, EndMessage, CR, LF };
        public static readonly byte[] AckMessage = new byte[6] { Ack, 0x30, 0x30, 0x30, CR, LF };

        public const byte IdentificationLength = 12;
        public const byte ObisCodeMaxLength = 16;
        public const byte ObisValueMaxLength = 32;
        public const byte ObisUnitMaxLength = 16;
    }
}
