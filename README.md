# blqw.Logging
日志框架([`Microsoft.Extensions.Logging`](https://www.baidu.com/s?ie=UTF-8&wd=Microsoft.Extensions.Logging))拓展功能

[blogs](https://www.jianshu.com/p/e01af9fa77e8)
## Demo
```
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
```

> 2018-08-27 15:38:41.940 【Debug】 xxxxx (Program.Main:18)  
> 2018-08-27 15:38:42.139 【Trace】 yyyyy (System.Diagnostics.TraceListener)  
## 更新说明 

#### [1.0.1] 2018.09.01
* 调整代码

#### [1.0.0] 2018.08.27
* 更新说明