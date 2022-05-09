using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ConsoleApp1
{
    class BridgeLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _baseFactory;

        private Func<string, object[], (string, object[])> _logFormatter;

        public BridgeLoggerFactory()
        {
            _baseFactory = new LoggerFactory();
        }

        public void AddProvider(ILoggerProvider provider) => _baseFactory.AddProvider(provider);

        public ILogger CreateLogger(string categoryName) => _baseFactory.CreateLogger(categoryName);

        public void Dispose()
        {
            _baseFactory.Dispose();
        }

        public void AddLoggerAsProvider(ILogger targetLogger)
        {
            var provider = new BridgeLoggerProvider(targetLogger, _logFormatter);
            AddProvider(provider);
        }

        public void SetLogFormatter(Func<string, object[], (string, object[])> logFormatter)
        {
            _logFormatter = logFormatter;
        }
    }

    public class BridgeLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public BridgeLoggerProvider(ILogger targetLogger, Func<string, object[], (string, object[])> logFormatter)
        {
            _logger = new BridgeLogger(targetLogger, logFormatter);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }

    public class BridgeLogger : ILogger
    {
        private readonly ILogger _targetLogger;
        private readonly Func<string, object[], (string, object[])> _logFormatter;

        public BridgeLogger(ILogger targetLogger, Func<string, object[], (string, object[])> logFormatter)
        {
            _targetLogger = targetLogger;
            _logFormatter = logFormatter;
        }

        public IDisposable BeginScope<TState>(TState state) => _targetLogger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _targetLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            object[] stateArgs = null;

            var (format, args) = GetLogFormat<TState>(state);

            if (_logFormatter == null)
            {
                stateArgs = new object[] {
                    format,
                    args
                };
            }
            else
            {
                var (newFormat, newArgs) = _logFormatter(format, args);

                stateArgs = new object[] {
                    newFormat,
                    newArgs
                };
            }

            state = (TState)Activator.CreateInstance(state.GetType(), stateArgs);

            _targetLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        private (string, object[]) GetLogFormat<TState>(TState state)
        {
            var stateFields = state.GetType().GetRuntimeFields();

            var args = (object[])stateFields.FirstOrDefault(o => o.Name == "_values")?
                .GetValue(state);

            var originalMessage = (string)stateFields.FirstOrDefault(o => o.Name == "_originalMessage")?
                .GetValue(state);

            var stateFormatter = stateFields.FirstOrDefault(o => o.Name == "_formatter")
                .GetValue(state);

            if (args == null || args.Length == 0 || stateFormatter == null)
            {
                return (originalMessage, args);
            }
            else
            {
                var formatterProperties = stateFormatter.GetType().GetRuntimeProperties();

                var originalFormat = (string)formatterProperties.FirstOrDefault(o => o.Name == "OriginalFormat")?.GetValue(stateFormatter);

                return (originalFormat, args);
            }
        }
    }
}
