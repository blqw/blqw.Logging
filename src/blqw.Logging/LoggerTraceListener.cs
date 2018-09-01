using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace blqw.Logging
{
    class LoggerTraceListener : TraceListener
    {
        public LoggerTraceListener(ILogger logger) => Logger = logger;

        public ILogger Logger { get; }

        public override void Write(string message) =>
            Logger.Log(LogLevel.Trace, new EventId(0, "Trace.Write"), message, null, (a, b) => a);

        public override void WriteLine(string message) =>
            Logger.Log(LogLevel.Trace, new EventId(0, "Trace.WriteLine"), message, null, (a, b) => a);
    }
}
