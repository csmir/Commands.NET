using Commands.Conditions;
using Commands.Core;
using Commands.Helpers;
using Commands.TypeConverters;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command.
    /// </summary>
    public sealed class CommandInfo : IConditional, IArgumentBucket
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public bool IsQueryable { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public PreconditionAttribute[] Preconditions { get; }

        /// <inheritdoc />
        public PostconditionAttribute[] PostConditions { get; }

        /// <inheritdoc />
        public IArgument[] Arguments { get; }

        /// <inheritdoc />
        public bool HasArguments { get; }

        /// <inheritdoc />
        public bool HasRemainder { get; }

        /// <inheritdoc />
        public int MinLength { get; }

        /// <inheritdoc />
        public int MaxLength { get; }

        /// <summary>
        ///     Gets the priority of this command.
        /// </summary>
        public byte Priority { get; }

        /// <summary>
        ///     Gets the module in which the command is known.
        /// </summary>
        public ModuleInfo Module { get; }

        /// <summary>
        ///     Gets the invocation target of this command.
        /// </summary>
        public MethodInfo Target { get; }

        internal CommandInfo(
            ModuleInfo module, MethodInfo method, string[] aliases, BuildOptions options)
        {
            var attributes = method.GetAttributes(true);
            var preconditions = attributes.GetPreconditions();
            var postconditions = attributes.GetPostconditions();

            var parameters = method.GetParameters(options);

            var (minLength, maxLength) = parameters.GetLength();

            if (parameters.Any(x => x.Attributes.Contains<RemainderAttribute>(false)))
            {
                if (parameters.Length > 1 && parameters[^1].IsRemainder)
                {
                    ThrowHelpers.ThrowInvalidOperation($"{nameof(RemainderAttribute)} can only exist on the last parameter of a method.");
                }
            }

            Priority = attributes.SelectFirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

            Target = method;
            Module = module;

            Attributes = attributes;
            Preconditions = preconditions;
            PostConditions = postconditions;

            Arguments = parameters;
            HasArguments = parameters.Length > 0;
            HasRemainder = parameters.Any(x => x.IsRemainder);

            Name = aliases[0];
            Aliases = aliases;
            IsQueryable = true;

            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(true);

        /// <inheritdoc cref="ToString()"/>
        /// <param name="withModuleInfo">Defines if the module information should be appended on the command level.</param>
        public string ToString(bool withModuleInfo)
        {
            return $"{(withModuleInfo ? $"{Module}." : "")}{Target.Name}['{Name}']({string.Join<IArgument>(", ", Arguments)})";
        }
    }
}
