using Commands.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    [Name("class-based", "cb")]
    public class ClassModule : ModuleBase
    {
        public void Run()
        {
            Console.WriteLine("Success!");
        }
    }
}
