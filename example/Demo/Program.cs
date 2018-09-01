using blqw.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new ServiceCollection()
                                .AddLogging(x => x.SetMinimumLevel(0))
                                .BuildServiceProvider();
            provider.TraceListenerToLogger();
            provider.AddConsoleLogger();


            var logger = provider.GetLogger();
            Trace.WriteLine("trace test");
            logger.Debug("debug test");
            try
            {
                var i = 0;
                i = i / i;
            }
            catch (Exception e)
            {
                logger.Error(new Exception("测试错误", e), "error test");
            }
        }
    }
}





