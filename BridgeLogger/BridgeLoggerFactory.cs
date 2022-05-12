using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BridgeLogger
{
    public class BridgeLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _baseFactory = new LoggerFactory();
        private List<IBridgeLoggerProvider> _bridgeProviders = new List<IBridgeLoggerProvider>();
        private Func<string, List<object>, (string, object[])> _logFormatter;

        public BridgeLoggerFactory()
        {
        }

        public void AddProvider(ILoggerProvider provider) => _baseFactory.AddProvider(provider);

        public ILogger CreateLogger(string categoryName) => _baseFactory.CreateLogger(categoryName);

        public void Dispose()
        {
            _baseFactory.Dispose();
        }

        public void AddLoggerAsProvider<T>(ILogger<T> targetLogger)
        {
            var provider = new BridgeLoggerProvider<T>(targetLogger, _logFormatter);
            AddProvider(provider);

            _bridgeProviders.Add(provider);
        }

        public void SetLogFormatter(Func<string, List<object>, (string, object[])> logFormatter)
        {
            _logFormatter = logFormatter;

            foreach (var provider in _bridgeProviders)
            {
                provider.SetLogFormatter(_logFormatter);
            }
        }
    }
}
