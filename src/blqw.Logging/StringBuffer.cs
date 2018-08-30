using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace blqw.Logging
{
    class StringBuffer : IDisposable
    {
        const int MAX_CAPACITY = 15;
        static readonly ConcurrentQueue<StringBuilder> _cache = new ConcurrentQueue<StringBuilder>();
        static int _count = 0;

        public static StringBuffer Pop(out StringBuilder builder)
        {
            if (_count < MAX_CAPACITY && Interlocked.Increment(ref _count) < MAX_CAPACITY)
            {
                _cache.TryDequeue(out builder);
                if (builder == null)
                {
                    builder = new StringBuilder();
                }
                return new StringBuffer(builder);
            }
            builder = new StringBuilder();
            return new StringBuffer(null);
        }

        public StringBuffer(StringBuilder builder) => _stringBuilder = builder;

        private readonly StringBuilder _stringBuilder;
        public void Dispose()
        {
            if (_stringBuilder != null)
            {
                _stringBuilder.Clear();
                _cache.Enqueue(_stringBuilder);
            }
        }
    }
}
