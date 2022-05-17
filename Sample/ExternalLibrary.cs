using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Sample
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

        public void WriteSomeLog()
        {
            var dustin = new
            {
                Name = "Dustin"
            };

            var anotherDustin = new
            {
                Name = "AnotherDustin"
            };

            _logger?.LogInformation("{@Someone} {@Action} {@SomeoneElse}", dustin, "Punches", anotherDustin);
        }
    }
}
