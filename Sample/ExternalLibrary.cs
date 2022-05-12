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
            _logger?.LogInformation("{@Someone} {@Action} {@SomeoneElse}", "Dustin", "Punches", "Other Dustin");
        }
    }
}
