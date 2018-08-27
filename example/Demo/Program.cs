using blqw.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = new ServiceCollection()
                .AddLogging(x => x.SetMinimumLevel(0))
                .BuildServiceProvider()
                .AddConsoleLogger()
                .TraceListenerToLogger()
                .GetLogger<Program>();
            log.Debug("xxxxx");
            Trace.WriteLine("yyyyy");
        }
    }
}
