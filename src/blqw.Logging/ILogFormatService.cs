using System;
using System.Collections.Generic;
using System.Text;

namespace blqw.Logging
{
    public interface ILogFormatService
    {
        Func<object, Exception, string> GetFormatter(Type type);
    }
}
