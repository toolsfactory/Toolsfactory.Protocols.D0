using System;
using Microsoft.Extensions.Logging;

namespace Tiveria.Home.D0.SampleApp
{
    public class LoggingExtensionsLogger : Tiveria.Common.Logging.ILogger, Common.Logging.IHexDumpLogWriter
    {
        #region static configuration
        public static bool UseErrorOutputStream = true;
        private readonly Microsoft.Extensions.Logging.ILogger _extLogger;
        #endregion

        #region public properties
        public bool IsDebugEnabled => true;
        public bool IsInfoEnabled => true;
        public bool IsWarnEnabled => true;
        public bool IsErrorEnabled => true;
        public bool IsFatalEnabled => true;
        public bool IsTraceEnabled => true;
        #endregion

        public LoggingExtensionsLogger(Microsoft.Extensions.Logging.ILogger extLogger)
        {
            _extLogger = extLogger;
        }

        public void Debug(object message)
        {
            _extLogger.LogDebug(message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            _extLogger.LogDebug(exception, message.ToString());
        }

        public void Info(object message)
        {
            _extLogger.LogInformation(message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            _extLogger.LogInformation(exception, message.ToString());
        }

        public void Warn(object message)
        {
            _extLogger.LogWarning(message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            _extLogger.LogWarning(exception, message.ToString());
        }

        public void Error(object message)
        {
            _extLogger.LogError(message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            _extLogger.LogError(exception, message.ToString());
        }

        public void Fatal(object message)
        {
            _extLogger.LogCritical(message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            _extLogger.LogCritical(exception, message.ToString());
        }

        public void Trace(object message)
        {
            _extLogger.LogTrace(message.ToString());
        }

        public void Trace(object message, Exception exception)
        {
            _extLogger.LogTrace(exception, message.ToString());
        }

        public void WriteLine(string line)
        {
            _extLogger.Log(LogLevel.Trace, $"[HEXDUMP] {line}");
        }
    }
}
