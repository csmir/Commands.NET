using Commands.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Reflection
{
    public interface IInvokable
    {
        public MethodInfo Target { get; }

        public ValueTask<InvokeResult> InvokeAsync(ConsumerBase consumer, CommandInfo command, object?[] args, CommandOptions options);
    }
}
