namespace Commands.Conditions
{
    /// <summary>
    ///     A trigger or set of triggers that represents when a condition should be evaluated.
    /// </summary>
    [Flags]
    public enum ConditionTrigger
    {
        /// <summary>
        ///     The condition is never evaluated.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The condition is evaluated before the conversion of arguments to the command's parameters.
        /// </summary>
        BeforeConversion = 1 << 0,

        /// <summary>
        ///     The condition is evaluated after the conversion of arguments to the command's parameters, and before the invocation of the command.
        /// </summary>
        BeforeInvocation = 1 << 1,

        /// <summary>
        ///     The condition is evaluated after the invocation of the command.
        /// </summary>
        AfterInvocation = 1 << 2,
    }
}
