using blqw.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


[assembly: AssemblyStartup(typeof(Startup))]
namespace blqw.Logging
{
    static class Startup
    {
        static IList<ILogFormatService> _formatServices;  // 注入
        static readonly ConcurrentDictionary<Type, Func<object, Exception, string>> _formatServicesCache =
                    new ConcurrentDictionary<Type, Func<object, Exception, string>>();
        public static Func<object, Exception, string> GetFormatter(Type type)
        {
            return _formatServicesCache.GetOrAdd(type, t =>
            {
                var services = _formatServices;
                if (services != null && services.Count > 0)
                {
                    for (var i = services.Count - 1; i >= 0; i--)
                    {
                        var formatter = services[i].GetFormatter(t);
                        if (formatter != null)
                        {
                            return formatter;
                        }
                    }
                }
                return null;
            });
        }

        /// <summary>
        /// 安装服务
        /// </summary>
        private static void Configure(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IEnumerable<ILogFormatService>)) is IEnumerable<ILogFormatService> formatters)
            {
                _formatServices = new List<ILogFormatService>(formatters).AsReadOnly();
            }
        }
    }
}