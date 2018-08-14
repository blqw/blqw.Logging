using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var www = WebRequest.CreateHttp("http://172.16.9.116:32770/fc/api/tranweight/save");
            www.Method = "POST";
            www.ContentType = "application/x-www-form-urlencoded";
            using(var stream = new StreamWriter(www.GetRequestStream()))
            {
                stream.Write("param=" + Uri.EscapeDataString("{\"data\":{\"BILL_LINE\":\"BOX0118052900007A_347\",\"WEIGHT\":\"0.23\"}}"));
                stream.Flush();
                using (var res = (HttpWebResponse)www.GetResponseAsync().Result)
                {
                    using (var reader = new StreamReader(res.GetResponseStream()))
                    {
                        var s = reader.ReadToEnd();
                        Console.WriteLine(s);
                    }
                }

            }




            var a = new ServiceCollection();
            a.AddLogging();
            a.AddSingleton<ILoggerFactory>(p => new LogFactory(p));
            var b = a.BuildServiceProvider();
            var c = b.GetRequiredService<ILoggerFactory>();
            var d = c.CreateLogger("test");
        }

        class LogFactory : ILoggerFactory
        {
            private Lazy<ILoggerFactory> _innerFactory;
            public LogFactory(IServiceProvider p)
            {
                _innerFactory = new Lazy<ILoggerFactory>(() => p.GetServices<ILoggerFactory>().Where(x => !(x is LogFactory)).FirstOrDefault(), LazyThreadSafetyMode.PublicationOnly);
            }

            public ILogger CreateLogger(string categoryName) => _innerFactory.Value?.CreateLogger(categoryName);
            public void AddProvider(ILoggerProvider provider) => _innerFactory.Value?.AddProvider(provider);
            public void Dispose() => _innerFactory.Value?.Dispose();
        }
    }
}
