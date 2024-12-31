using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    [Name("module")]
    public class Module : CommandModule
    {
        [Name("command1")]
        public string ACommand()
            => "Test";

        [Name("command2")]
        public void ACommand(params int[] arg)
            => Console.WriteLine(string.Join(", ", arg));

        [Name("command3")]
        public Task<string> Respond(string str)
            => Task.FromResult(str);

        public class CallerContext : ICallerContext
        {
            public Task Respond(object? response)
            {
                Console.WriteLine(response);

                return Task.CompletedTask;
            }
        }
    }
}
