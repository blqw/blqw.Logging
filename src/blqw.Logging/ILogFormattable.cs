using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace blqw.Logging
{

    public interface ILogFormattable
    {
        string Log<TState>(ILogger logger, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);

        string BeginScope<TState>(ILogger logger, TState state);

        string EndScope<TState>(ILogger logger, TState state);
    }
}