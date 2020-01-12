namespace Tiveria.Home.D0
{
    public class ObisData
    {
        public string Raw { get; private set; }
        public string Code { get; private set; }
        public string Value { get; private set; }
        public string Unit { get; private set; }

        public ObisData(string raw, string code, string value, string unit = "")
        {
            Raw = raw;
            Code = code;
            Value = value;
            Unit = unit;
        }
    }
}
