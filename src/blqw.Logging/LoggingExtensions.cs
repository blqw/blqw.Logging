using blqw;
using blqw.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace blqw.Logging
{
    /// <summary>
    /// 日志相关扩展方法
    /// </summary>
    public static class LoggingExtensions
    {
        private static string GetEventName(string path, string member, int line = 0)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return member?.Trim() + (line == 0 ? "" : ":" + line);
            }
            if (string.IsNullOrWhiteSpace(member))
            {
                return path?.Trim() + (line == 0 ? "" : ":" + line);
            }
            return Path.GetFileNameWithoutExtension(path.Trim()) + "." + member.Trim() + (line == 0 ? "" : ":" + line);
        }

        private static ILogger DefaultLogger { get; } = new ConsoleLogger(null);

        private static string DefaultFormatter(object state, Exception exception)
        {
            var formatter = GetFormatter(state?.GetType());
            if (formatter != null)
            {
                return formatter(state, exception);
            }
            var message = GetStateString(state);
            if (exception == null)
            {
                return message;
            }
            using (StringBuffer.Pop(out var builder))
            {
                builder.Append(message);
                WriteExceptionString(builder, exception);
                return builder.ToString();
            }
        }

        private static string GetStateString(object state)
        {
            if (state == null)
            {
                return "<null>";
            }

            switch (state)
            {
                case IFormattable b:
                    return b.ToString(null, null);
                case IConvertible c:
                    return c.ToString(null);
                default:
                    return $"{state.ToString()}({state.GetType()})";
            }
        }

        private static void WriteExceptionString(StringBuilder builder, Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            //循环输出异常
            while (exception != null)
            {
                builder.AppendLine();
                builder.AppendLine($"{exception.ToString()}");
                // 获取基础异常
                var ex = exception.GetBaseException();
                // 基础异常获取失败则获取 内部异常
                if (ex == null || ex == exception)
                {
                    // 预防出现一些极端例子导致死循环
                    if (ex == exception.InnerException)
                    {
                        return;
                    }
                    ex = exception.InnerException;
                }
                exception = ex;
            }
        }

        private static Func<object, Exception, string> GetFormatter(Type type)
        {
            return null;
        }


        /// <summary>
        /// 标准日志输出
        /// </summary>
        public static void Write(this ILogger logger, LogLevel logLevel, object messageOrObject,
                                Exception exception = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(logLevel, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 错误日志输出
        /// </summary>
        public static void Error(this ILogger logger, Exception exception,
                                object messageOrObject = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(LogLevel.Error, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 严重错误日志输出
        /// </summary>
        public static void Critical(this ILogger logger, Exception exception,
                                object messageOrObject = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(LogLevel.Critical, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 标准日志输出
        /// </summary>
        public static void Log(this ILogger logger, object messageOrObject,
                                Exception exception = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(LogLevel.Trace, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 调试日志输出
        /// </summary>
        public static void Debug(this ILogger logger, object messageOrObject,
                                Exception exception = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(LogLevel.Debug, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 普通信息日志输出
        /// </summary>
        public static void Info(this ILogger logger, object messageOrObject,
                                Exception exception = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(LogLevel.Information, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 警告日志输出
        /// </summary>
        public static void Warn(this ILogger logger, object messageOrObject,
                                Exception exception = null,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
                => (logger ?? DefaultLogger).Log(LogLevel.Warning, new EventId(sourceLineNumber, GetEventName(sourceFilePath, memberName)), messageOrObject, exception, DefaultFormatter);

        /// <summary>
        /// 将通过 <see cref="Trace"/> 记录的内容转发到日志
        /// </summary>
        public static IServiceProvider TraceListenerToLogger(this IServiceProvider serviceProvider)
        {
            if (serviceProvider?.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
            {
                var categoryName = TypeNameHelper.GetTypeDisplayName(typeof(TraceListener));
                var logger = factory.CreateLogger(categoryName);
                WatchTrace(logger);
            }
            return serviceProvider;
        }

        private static ConcurrentDictionary<IServiceProvider, ILoggerProvider> _loggerProviders = new ConcurrentDictionary<IServiceProvider, ILoggerProvider>();
        public static IServiceProvider AddConsoleLogger(this IServiceProvider serviceProvider)
        {
            if (_loggerProviders.TryAdd(serviceProvider, ConsoleLogger.LoggerProvider))
            {
                if (serviceProvider.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
                {
                    factory.AddProvider(ConsoleLogger.LoggerProvider);
                    return serviceProvider;
                }

                throw new Exception("缺少服务 ILoggerFactory");
            }
            else if (serviceProvider.GetService(typeof(ILoggerFactory)) == null)
            {
                throw new Exception("缺少服务 ILoggerFactory");
            }
            return serviceProvider;
        }

        /// <summary>
        /// 获取日志服务
        /// </summary>
        public static ILogger GetLogger(this IServiceProvider serviceProvider, string categoryName)
        {
            if (serviceProvider?.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
            {
                return factory.CreateLogger(categoryName);
            }
            //如果不存在任何服务, 则返回默认服务
            return new ConsoleLogger(categoryName);
        }

        /// <summary>
        /// 获取日志服务
        /// </summary>
        public static ILogger GetLogger(this IServiceProvider serviceProvider, Type type) =>
            serviceProvider.GetLogger(TypeNameHelper.GetTypeDisplayName(type));

        /// <summary>
        /// 获取日志服务
        /// </summary>
        public static ILogger GetLogger<T>(this IServiceProvider serviceProvider) =>
            serviceProvider.GetLogger(TypeNameHelper.GetTypeDisplayName(typeof(T)));

        /// <summary>
        /// 配置日志
        /// </summary>
        public static IServiceProvider ConfigLogger(this IServiceProvider serviceProvider, Action<ILoggerFactory> configure)
        {
            if (serviceProvider?.GetService(typeof(ILoggerFactory)) is ILoggerFactory factory)
            {
                configure(factory);
            }
            return serviceProvider;
        }

        /// <summary>
        /// 观察控制台输出，并转发到指定的日志组件
        /// </summary>
        /// <param name="logger"></param>
        private static void WatchTrace(this ILogger logger)
        {
            if (Trace.Listeners.OfType<LoggerTraceListener>().Any(x => x.Logger == logger))
            {
                return;
            }
            Trace.Listeners.Add(new LoggerTraceListener(logger));
        }
    }
}
