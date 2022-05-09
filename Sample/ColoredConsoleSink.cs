using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace ConsoleApp1
{
    public class ColoredConsoleSink : ILogEventSink
    {
        private readonly ConsoleColor _foregroundColor;
        private readonly ConsoleColor _backgroundColor;

        private readonly ITextFormatter _formatter;

        public ColoredConsoleSink(ITextFormatter formatter, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            _formatter = formatter;
            _foregroundColor = foregroundColor;
            _backgroundColor = backgroundColor;
        }

        public void Emit(LogEvent logEvent)
        {
            var originalForegroundColor = Console.ForegroundColor;
            var originalBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = _foregroundColor;
            Console.BackgroundColor = _backgroundColor;

            _formatter.Format(logEvent, Console.Out);
            Console.Out.Flush();

            Console.ForegroundColor = originalForegroundColor;
            Console.BackgroundColor = originalBackgroundColor;
        }
    }

    public static class ColoredConsoleSinkExtensions
    {
        public static LoggerConfiguration ColoredConsole(
            this LoggerSinkConfiguration loggerConfiguration,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            IFormatProvider formatProvider = null, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            var textFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var coloredConsoleSink = new ColoredConsoleSink(textFormatter, foregroundColor, backgroundColor);
            return loggerConfiguration.Sink(coloredConsoleSink, minimumLevel);
        }
    }
}
