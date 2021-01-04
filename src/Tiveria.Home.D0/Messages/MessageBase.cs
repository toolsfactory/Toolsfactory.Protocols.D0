using System;
using System.Collections.Generic;
using System.Text;
using Tiveria.Common.Builders;

namespace Tiveria.Home.D0.Messages
{
    public class MessageGenerator
    {
        private static readonly ByteArrayBuilder _buffer = new ByteArrayBuilder();
        public static byte[] GenerateRequestMessage(string deviceAddr="")
        {
            _buffer.Clear();
            _buffer.Append(Constants.StartMessage);
            _buffer.Append(Constants.Request);
            if (!String.IsNullOrEmpty(deviceAddr))
                _buffer.Append(deviceAddr);
            _buffer.Append(Constants.EndMessage);
            _buffer.Append(Constants.CR);
            _buffer.Append(Constants.LF);
            return _buffer.ToArray();
        }
    }
}
