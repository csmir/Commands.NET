using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    /// <summary>
    ///     An attribute that signifies that a module or command should <b>not</b> be registered automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DoNotRegister : Attribute
    {

    }
}
