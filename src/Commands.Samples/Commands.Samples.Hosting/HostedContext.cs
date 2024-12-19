using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Samples
{
    public sealed class HostedContext : ICallerContext
    {
        public Task Respond(object? response)
        {
            Console.WriteLine(response);

            return Task.CompletedTask;
        }
    }
}
