using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ConsoleApp1
{
    class ExternalLibrary
    {
        private readonly ILogger<ExternalLibrary> _logger;

        public ExternalLibrary(ILogger<ExternalLibrary> logger)
        {
            _logger = logger;
        }

        public ExternalLibrary(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExternalLibrary>();
        }

        public void DoSomething()
        {
            _logger?.LogInformation("{Someone} {Action} {SomeoneElse}", "Dustin", "Punches", "Other Dustin");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var baseFactory = ConfigureLog();

            TryWithLogFactory(baseFactory);
            TryWithLogger(baseFactory);

            Console.ReadKey();
        }

        static ILoggerFactory ConfigureLog()
        {
            var seriLogger1 = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var seriLogger2 = new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.ColoredConsole(foregroundColor: ConsoleColor.Red)
                .CreateLogger();

            var baseFactory = new LoggerFactory();
            baseFactory.AddSerilog(seriLogger1);
            baseFactory.AddSerilog(seriLogger2);

            return baseFactory;
        }

        static void TryWithLogFactory(ILoggerFactory baseFactory)
        {
            BeginSection();

            var baseLogger = baseFactory.CreateLogger<ExternalLibrary>();

            var bridgeFactory = new BridgeLoggerFactory();
            // LogFormatter must be set before adding providers,
            // otherwise the formatter won't get invoked
            bridgeFactory.SetLogFormatter(CustomLogFormatter);
            bridgeFactory.AddLoggerAsProvider(baseLogger);

            var externalLibrary = new ExternalLibrary(bridgeFactory);
            externalLibrary.DoSomething();

            EndSection();
        }

        static void TryWithLogger(ILoggerFactory logFactory)
        {
            BeginSection();

            var logger = logFactory.CreateLogger<ExternalLibrary>();

            var externalLibrary = new ExternalLibrary(logger);
            externalLibrary.DoSomething();

            EndSection();
        }

        static (string, object[]) CustomLogFormatter(string format, object[] args)
        {
            var newFormat = "{@Module} " + format;

            var newArgList = args.ToList();
            newArgList.Insert(0, "ExternalLibrary");
            var newArgs = newArgList.ToArray();

            return (newFormat, newArgs);
        }

        static void BeginSection([CallerMemberName] string name = "")
        {
            Console.WriteLine("****************************************");
            Console.WriteLine(name);
            Console.WriteLine("****************************************");
        }

        static void EndSection()
        {
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
