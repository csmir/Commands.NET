namespace Commands.Tests
{
    public class ConditionModule : CommandModule
    {
        [Name("condition-or")]
        // Grouped by OR, the command will succeed if any of the conditions are met. This scenario will succeed.
        [ORCondition(true)]
        [ORCondition(false)]
        public static string ConditionOR()
        {
            return "Success";
        }

        [Name("condition-and")]
        // Grouped by AND, the command will succeed if all of the conditions are met. This scenario will fail.
        [ANDCondition(true)]
        [ANDCondition(false)]
        public static string ConditionAND()
        {
            return "Success";
        }

        [Name("condition-or-and")]
        // Grouped by OR. This scenario will succeed.
        [ORCondition(true)]
        [ORCondition(false)]
        // Grouped by AND. This scenario will fail.
        [ANDCondition(true)]
        [ANDCondition(true)]
        public static string ConditionORAND()
        {
            return "Success";
        }
    }
}
