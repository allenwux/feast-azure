using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Utils
{
    public static class LoggingExtensions
    {

        public static IDisposable BeginNamedScope(this ILogger logger,
            FeastCoreRequestHeaders header)
        {
            var dictionary = header.GetLoggingScopeProperties();
            return logger.BeginScope(dictionary);
        }

        public static void LogMethodBegin(this ILogger logger, string methodName)
        {
            logger.LogInformation($"[FxBegin][{methodName}] Function {methodName} begins.");
        }

        public static void LogMethodEnd(this ILogger logger, string methodName)
        {
            logger.LogInformation($"[FxEnd][{methodName}] Function {methodName} ends.");
        }
    }
}
