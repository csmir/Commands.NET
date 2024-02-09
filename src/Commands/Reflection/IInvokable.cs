using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Reflection
{
    public interface IInvokable
    {
        public bool IsDelegate { get; }

        public object? Invoke(object? context, params object[]? args);
    }
}
