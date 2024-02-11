using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Core
{
    internal enum ResultCode
    {
        Success = 0,

        NotFound = 1,

        Incomplete = 2,

        PreconditionError,

        ArgumentMismatch,

        ConvertError,

        InvokeFailed,

        PostconditionError
    }
}
