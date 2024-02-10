using Commands.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    [Name("score")]
    public class ScoreModule : ModuleBase
    {
        [Name("command")]
        public void ByBool(bool b)
        {
            Console.WriteLine("ByString " + b);
        }

        [Name("command")]
        public void ByInt(int i)
        {
            Console.WriteLine("ByInt " + i);
        }

        public void Default()
        {
            Console.WriteLine("This is a default overload");
        }

        public void Default([Remainder] string args)
        {
            Console.WriteLine($"This is a default overload with args: {args}");
        }
    }
}
