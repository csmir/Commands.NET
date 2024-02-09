using Commands.Conditions;
using Commands.Core;
using Commands.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Reflection.Impl
{
    public sealed class DelegateCommandInfo : IConditional, IArgumentBucket, IInvokable
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

        /// <inheritdoc />
        public byte Priority { get; }

        /// <inheritdoc />
        public bool IsDelegate { get; }

        /// <summary>
        ///     Gets the module in which the command is known.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if the command is <see langword="static"/> or a <see langword="delegate"/>.
        /// </remarks>
        public ModuleInfo? Module { get; }

        /// <summary>
        ///     Represents a delegate that will 
        /// </summary>
        public Delegate Target { get; }

        internal DelegateCommandInfo(Delegate action, string[] aliases, BuildOptions options)
        {
            IsQueryable = true;
            IsDelegate = true;

            var attributes = action.Method.GetAttributes(true);
            var preconditions = attributes.GetPreconditions();
            var postconditions = attributes.GetPostconditions();

            var parameters = action.Method.GetParameters(options);

            var (minLength, maxLength) = parameters.GetLength();

            if (parameters.Any(x => x.Attributes.Contains<RemainderAttribute>(false)))
            {
                if (parameters.Length > 1 && parameters[^1].IsRemainder)
                {
                    ThrowHelpers.ThrowInvalidOperation($"{nameof(RemainderAttribute)} can only exist on the last parameter of a method.");
                }
            }

            Priority = attributes.SelectFirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

            Target = action;

            Attributes = attributes;
            Preconditions = preconditions;
            PostConditions = postconditions;

            Arguments = parameters;
            HasArguments = parameters.Length > 0;
            HasRemainder = parameters.Any(x => x.IsRemainder);

            Name = aliases[0];
            Aliases = aliases;

            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <inheritdoc />
        public object? Invoke(object? context, params object[]? args)
        {
            return Target.DynamicInvoke([context, ..args]);
        }
    }
}
