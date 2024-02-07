using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Core
{
    public struct BuildingContext()
    {
        public Assembly[] Assemblies { get; set; } = [Assembly.GetEntryAssembly()];
    }
}
