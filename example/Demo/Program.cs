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

            provider.ConfigLogger(x =>
            {
                x.AddConsole(0);
                x.AddDebug(0);
            });

            var logger = provider.GetLogger<Program>();

            logger.Debug("debug test");
            logger.Error(new Exception("测试错误"), "error test");
        }
    }
}





