using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BridgeLogger
{
    public interface IBridgeLogger
    {
        public void SetLogFormatter(Func<string, List<object>, (string, object[])> logFormatter);
    }

    public class BridgeLogger<T> : ILogger<T>, IBridgeLogger
    {
        private readonly ILogger<T> _targetLogger;
        private Func<string, List<object>, (string, object[])> _logFormatter;

        public BridgeLogger(ILogger<T> targetLogger, Func<string, List<object>, (string, object[])> logFormatter = null)
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

        public void SetLogFormatter(Func<string, List<object>, (string, object[])> logFormatter)
        {
            _logFormatter = logFormatter;
        }

        private (string, List<object>) GetLogFormat<TState>(TState state)
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
                return (originalMessage, new List<object>());
            }
            else
            {
                var formatterProperties = stateFormatter.GetType().GetRuntimeProperties();

                var originalFormat = (string)formatterProperties.FirstOrDefault(o => o.Name == "OriginalFormat")?.GetValue(stateFormatter);

                return (originalFormat, args.ToList());
            }
        }
    }
}
