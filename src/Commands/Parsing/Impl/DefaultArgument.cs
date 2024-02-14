using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Parsing
{
    public readonly struct DefaultArgument(string? value) : IParseArgument
    {
        public object? Value { get; } = value;

        public static implicit operator DefaultArgument(string? value)
        {
            return new DefaultArgument(value);
        }
    }
}
