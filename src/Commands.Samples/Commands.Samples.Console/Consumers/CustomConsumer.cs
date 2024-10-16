using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Samples
{
    public class CustomConsumer : ConsumerBase
    {
        public string Name { get; }

        public CustomConsumer(string name)
        {
            Name = name;
        }
    }
}
