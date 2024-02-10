using Commands.Conditions;
using Commands.Core;
using Commands.Helpers;
using System.Dynamic;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command.
    /// </summary>
    public sealed class CommandInfo : IConditional, IArgumentBucket
    {
        /// <inheritdoc />
        public string? Name { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public bool IsQueryable { get; }

        /// <inheritdoc />
        public bool IsDefault { get; }

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

        /// <inheritdoc />
        public byte Priority { get; }

        /// <summary>
        ///     Gets the module in which the command is known.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> when the <see cref="IInvokable"/> of this command is <see cref="StaticInvoker"/> or <see cref="DelegateInvoker"/>.
        /// </remarks>
        public ModuleInfo? Module { get; }

        /// <summary>
        ///     Gets the invocation target of this command.
        /// </summary>
        public IInvokable Invoker { get; }

        /// <inheritdoc />
        public float Score
        {
            get
            {
                return GetScore();
            }
        }

        internal CommandInfo(StaticInvoker invoker, string[] aliases, BuildOptions options)
            : this(null, invoker, aliases, true, options)
        {
        }

        internal CommandInfo(DelegateInvoker invoker, string[] aliases, BuildOptions options)
            : this(null, invoker, aliases, true, options)
        {
        }

        internal CommandInfo(
            ModuleInfo? module, IInvokable invoker, string[] aliases, bool hasContext, BuildOptions options)
        {
            IsQueryable = true;

            var attributes = invoker.Target.GetAttributes(true);
            var preconditions = attributes.GetPreconditions();
            var postconditions = attributes.GetPostconditions();

            var parameters = invoker.Target.GetParameters(hasContext, options);

            if (hasContext)
            {
                if (parameters.Length == 0 || parameters[0].Type != typeof(CommandContext))
                {
                    ThrowHelpers.ThrowInvalidOperation($"A delegate or static command signature must implement {nameof(CommandContext)} as the first parameter.");
                }
            }

            var (minLength, maxLength) = parameters.GetLength();

            if (parameters.Any(x => x.Attributes.Contains<RemainderAttribute>(false)))
            {
                if (parameters.Length > 1 && parameters[^1].IsRemainder)
                {
                    ThrowHelpers.ThrowInvalidOperation($"{nameof(RemainderAttribute)} can only exist on the last parameter of a command signature.");
                }
            }

            Priority = attributes.SelectFirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

            Invoker = invoker;
            Module = module;

            Attributes = attributes;
            Preconditions = preconditions;
            PostConditions = postconditions;

            Arguments = parameters;
            HasArguments = parameters.Length > 0;
            HasRemainder = parameters.Any(x => x.IsRemainder);

            Aliases = aliases;

            if (aliases.Length > 0)
            {
                IsDefault = false;
                Name = aliases[0];
            }
            else
            {
                IsDefault = true;
                Name = null;
            }

            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <inheritdoc />
        public float GetScore()
        {
            var score = 1.0f;

            foreach (var argument in Arguments)
            {
                score += argument.GetScore();
            }

            score += Priority;

            return score;
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(true);

        /// <inheritdoc cref="ToString()"/>
        /// <param name="withModuleInfo">Defines if the module information should be appended on the command level.</param>
        public string ToString(bool withModuleInfo)
        {
            return $"{(withModuleInfo ? $"{Module}." : "")}{Invoker.Target.Name}{(Name != null ? $"['{Name}']" : "")}({string.Join<IArgument>(", ", Arguments)})";
        }
    }
}
