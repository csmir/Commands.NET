using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Parsing
{
    public interface IParseArgument
    {
        public object? Value { get; }
    }
}
