using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BridgeLogger
{
    public interface IBridgeLoggerProvider
    {
        public void SetLogFormatter(Func<string, List<object>, (string, object[])> logFormatter);
    }

    public class BridgeLoggerProvider<T> : ILoggerProvider, IBridgeLoggerProvider
    {
        private readonly ILogger<T> _logger;
        private readonly IBridgeLogger _bridgeLogger;

        public BridgeLoggerProvider(ILogger<T> targetLogger, Func<string, List<object>, (string, object[])> logFormatter = null)
        {
            var loggerInstance = new BridgeLogger<T>(targetLogger, logFormatter);
            _logger = loggerInstance;
            _bridgeLogger = loggerInstance;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }

        public void SetLogFormatter(Func<string, List<object>, (string, object[])> logFormatter)
        {
            _bridgeLogger.SetLogFormatter(logFormatter);
        }
    }
}
