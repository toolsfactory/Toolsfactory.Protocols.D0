namespace Toolsfactory.Protocols.D0
{
    public class VendorMessageEventArgs : System.EventArgs
    {
        public string Vendor { get; private set; } = "";
        public string Identification { get; private set; } = "";
        public int Baudrate { get; private set; } = 300;
        public char BaudrateCharacter { get; private set; } = '0';
        public bool UseShortReactionTime { get; private set; } = false;

        public VendorMessageEventArgs(string vendor, string identification, int baudrate, char baudrateCharacter, bool useShortReactionTime)
        {
            Vendor = vendor;
            Identification = identification;
            Baudrate = baudrate;
            BaudrateCharacter = baudrateCharacter;
            UseShortReactionTime = useShortReactionTime;
        }
    }
}