using Commands.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    internal class LoggingContext : CommandContext
    {
        public override void LogCritical(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("[Critical] " + string.Format(message, args));
            Console.ResetColor();
        }

        public override void LogError(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error] " + string.Format(message, args));
            Console.ResetColor();
        }

        public override void LogWarning(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Warn] " + string.Format(message, args));
            Console.ResetColor();
        }

        public override void LogInformation(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[Info] " + string.Format(message, args));
            Console.ResetColor();
        }

        public override void LogDebug(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[Debug] " + string.Format(message, args));
            Console.ResetColor();
        }

        public override void LogTrace(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("[Trace] " + string.Format(message, args));
            Console.ResetColor();
        }
    }
}
