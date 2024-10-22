using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    public class ConditionModule : ModuleBase
    {
        [Name("condition-or")]
        [ORCondition(true)]
        [ORCondition(false)]
        public string ConditionOR()
        {
            return "Success";
        }

        [Name("condition-and")]
        [ANDCondition(true)]
        [ANDCondition(false)]
        public string ConditionAND()
        {
            return "Success";
        }
    }
}
