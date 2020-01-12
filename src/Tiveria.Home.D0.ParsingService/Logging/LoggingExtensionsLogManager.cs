using Microsoft.Extensions.Logging;
using System;
using Tiveria.Common.Logging;

namespace Tiveria.Home.D0.SampleApp
{
    public class LoggingExtensionsLogManager : ILogManager
    {
        private readonly ILoggerFactory _logFactory;

        public LoggingExtensionsLogManager(Microsoft.Extensions.Logging.ILoggerFactory logFactory)
        {
            _logFactory = logFactory;
        }
        public Common.Logging.ILogger GetLogger(string name)
        {
            return new LoggingExtensionsLogger(_logFactory.CreateLogger(name));
        }

        public Common.Logging.ILogger GetLogger(Type type)
        {
            return new LoggingExtensionsLogger(_logFactory.CreateLogger(nameof(type)));
        }
    }
}
