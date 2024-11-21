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
