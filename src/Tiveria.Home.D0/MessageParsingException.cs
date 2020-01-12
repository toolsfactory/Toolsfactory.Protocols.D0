using System;
using System.Runtime.Serialization;

namespace Tiveria.Home.D0
{
    public partial class D0SimpleStreamParser
    {
        public class MessageParsingException : Exception
        {
            public MessageParsingException()
            {
            }

            public MessageParsingException(string message) : base(message)
            {
            }

            public MessageParsingException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected MessageParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
