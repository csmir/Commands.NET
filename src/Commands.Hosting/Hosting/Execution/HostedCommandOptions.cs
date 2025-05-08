using Commands.Conditions;

namespace Commands.Hosting;

/// <summary>
///     A set of options for handling command queries and determining the workflow in the command scope, from a hosted context. This class cannot be inherited.
/// </summary>
public sealed class HostedCommandOptions
{
    /// <summary>
    ///     Gets or sets whether the defined <see cref="ExecuteConditionAttribute{T}"/>'s for this execution should be ran.
    /// </summary>
    /// <remarks>
    ///     Default: <see langword="false"/>
    /// </remarks> 
    public bool SkipConditions { get; set; } = false;

    /// <summary>
    ///     Gets or sets the separator used to join remaining arguments in a command.
    /// </summary>
    /// <remarks>
    ///     Default: <c>' '</c>
    /// </remarks> 
    public char RemainderSeparator { get; set; } = ' ';

    /// <summary>
    ///     Gets or sets whether command execution should be handled asynchronously or not.
    /// </summary>
    /// <remarks>
    ///     This behavior drastically changes execution flow, and as such, there are a few things to consider when using it:
    ///     <list type="bullet">
    ///         <item>
    ///             The end-user must provide an implementation of <see cref="ResultHandler"/> to the <see cref="ExecutableComponentSet"/> that is used to execute the command, in order to handle the result of the command.
    ///         </item>
    ///         <item>
    ///             Objects, specifically those scoped to more than a single command must be made thread-safe, meaning they must be able to handle multiple requests at once.
    ///         </item>
    ///     </list>
    ///     When considering thread-safety, it is important to know what this actually means.
    ///     <br/>
    ///     Default: <see langword="true"/>
    /// </remarks>
    public bool ExecuteAsynchronously { get; set; } = true;
}
