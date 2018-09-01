using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace blqw.Logging
{
    /// <summary>
    /// <seealso cref="StringBuilder"/>对象池
    /// </summary>
    static class StringBuilderPool
    {
        /// <summary>
        /// 对象池最大容量大小
        /// </summary>
        public const int MAX_CAPACITY = 63;
        // 对象缓存
        private static readonly ConcurrentQueue<StringBuilder> _cache = new ConcurrentQueue<StringBuilder>();
        // 计数器
        private static int _counter = 0;
        /// <summary>
        /// 弹出 <seealso cref="StringBuilder"/> 对象
        /// </summary>
        public static IDisposable Pop(out StringBuilder builder)
        {
            // 尝试从缓存中获取对象
            _cache.TryDequeue(out builder);
            if (builder != null)
            {
                return new Recyclable(builder); // 返回可回收对象
            }

            // 计数器超过池最大容量,或计数器+1超过池最大容量 则直接返回新的 StringBuilder 且不回收
            if (_counter > MAX_CAPACITY || Interlocked.Increment(ref _counter) > MAX_CAPACITY)
            {
                builder = new StringBuilder();
                return NoRecycle; //不回收
            }
            builder = new StringBuilder();
            return new Recyclable(builder);
        }

        // 不执行回收操作的空对象
        private static readonly IDisposable NoRecycle = new Recyclable(null);

        /// <summary>
        /// 可回收 <seealso cref="StringBuilder"/> 的对象
        /// </summary>
        class Recyclable : IDisposable
        {
            /// <summary>
            /// 初始化对象
            /// </summary>
            /// <param name="builder">待回收的 <seealso cref="StringBuilder"/> 对象</param>
            public Recyclable(StringBuilder builder) => _stringBuilder = builder;
            // 待回收的对象
            private StringBuilder _stringBuilder;
            /// <summary>
            /// 回收对象
            /// </summary>
            public void Dispose()
            {
                // 将 _stringBuilder 修改为null 并返回 _stringBuilder 的原始值
                var builder = Interlocked.Exchange(ref _stringBuilder, null);
                if (builder != null)
                {
                    builder.Clear();
                    _cache.Enqueue(builder); //回收
                }
            }
            // 析构函数
            ~Recyclable() => Dispose();
        }

    }
}
