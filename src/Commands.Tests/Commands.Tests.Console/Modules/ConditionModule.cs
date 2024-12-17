namespace Commands.Tests
{
    public class ConditionModule : CommandModule
    {
        [Name("condition-or")]
        [ORCondition(true)]
        [ORCondition(false)]
        public static string ConditionOR()
        {
            return "Success";
        }

        [Name("condition-and")]
        [ANDCondition(true)]
        [ANDCondition(false)]
        public static string ConditionAND()
        {
            return "Success";
        }
    }
}
