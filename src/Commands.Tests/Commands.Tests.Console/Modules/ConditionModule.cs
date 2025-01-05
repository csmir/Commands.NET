namespace Commands.Tests;

public class ConditionModule : CommandModule
{
    [Name("condition-or")]
    // Grouped by OR, the command will succeed if any of the conditions are met. This scenario will succeed.
    [OR1(true)]
    [OR1(false)]
    public static string ConditionOR()
    {
        return "Success";
    }

    [Name("condition-and")]
    // Grouped by AND, the command will succeed if all of the conditions are met. This scenario will fail.
    [AND(true)]
    [AND(false)]
    public static string ConditionAND()
    {
        return "Success";
    }

    [Name("condition-or-and")]
    // Grouped by OR. This scenario will succeed.
    [OR1(true)]
    [OR1(false)]
    // Grouped by AND. This scenario will succeed.
    [AND(true)]
    [AND(true)]
    public static string ConditionORAND()
    {
        return "Success";
    }

    [Name("condition-or-multi")]
    // Grouped by OR, across multiple condition types. Because the evaluator (T) implementation of the conditions are the same, this scenario will succeed.
    [OR1(false)]
    [OR2(false)]
    [OR2(true)]
    public static string ConditionMultiOR()
    {
        return "Success";
    }
}
