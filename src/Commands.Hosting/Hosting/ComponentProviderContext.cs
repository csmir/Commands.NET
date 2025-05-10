using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Hosting;

public sealed class ComponentProviderContext
{
    public ComponentConfiguration Configuration { get; } = new();

    public ComponentProviderBuilder Components { get; } = new();
}
