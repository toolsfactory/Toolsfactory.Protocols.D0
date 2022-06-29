namespace Toolsfactory.Protocols.D0
{
    public class ObisDataEventArgs : System.EventArgs
    {
        public string Raw { get; private set; }
        public string Code { get; private set; }
        public string Value { get; private set; }
        public string Unit { get; private set; }

        public ObisDataEventArgs(string raw, string code, string value, string unit = "")
        {
            Raw = raw;
            Code = code;
            Value = value;
            Unit = unit;
        }

    }
}