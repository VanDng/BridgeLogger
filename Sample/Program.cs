using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BridgeLogger;
using Sample;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = InitializeLoggerFactory();

            Utility.Invoke(BridgeTheFactory, loggerFactory);
            Utility.Invoke(BridgeTheLogger, loggerFactory);

            Console.ReadKey();
        }

        static ILoggerFactory InitializeLoggerFactory()
        {
            var seriLogger1 = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var seriLogger2 = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.ColoredConsole(foregroundColor: ConsoleColor.Red)
                .CreateLogger();

            var factory = new LoggerFactory();
            factory.AddSerilog(seriLogger1);
            factory.AddSerilog(seriLogger2);

            return factory;
        }

        static void BridgeTheFactory(ILoggerFactory baseFactory)
        {
            var logger = baseFactory.CreateLogger<ExternalLibrary>();

            var bridgeFactory = new BridgeLoggerFactory();
            bridgeFactory.AddLoggerAsProvider(logger);
            bridgeFactory.SetLogFormatter(CustomLogFormatter);

            var externalLibrary = new ExternalLibrary(bridgeFactory);
            externalLibrary.WriteSomeLog();
        }

        static void BridgeTheLogger(ILoggerFactory logFactory)
        {
            var logger = logFactory.CreateLogger<ExternalLibrary>();

            var bridgeLogger = new BridgeLogger<ExternalLibrary>(logger, CustomLogFormatter);

            var externalLibrary = new ExternalLibrary(bridgeLogger);
            externalLibrary.WriteSomeLog();
        }

        static (string, object[]) CustomLogFormatter(string format, List<object> args)
        {
            var newFormat = $"{{@BridgeParam}} {format}";

            args.Insert(0, new
            {
                Message = "Bridge"
            });
            var newArgs = args.ToArray();

            return (newFormat, newArgs);
        }
    }
}
