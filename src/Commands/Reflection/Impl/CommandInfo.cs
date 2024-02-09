﻿using Commands.Conditions;
using Commands.Core;
using Commands.Helpers;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command.
    /// </summary>
    public sealed class CommandInfo : IConditional, IArgumentBucket, IInvokable
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
        ///     Gets if the command method is static or not.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        ///     Gets the module in which the command is known.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if the command is <see langword="static"/>.
        /// </remarks>
        public ModuleInfo? Module { get; }

        /// <summary>
        ///     Gets the invocation target of this command.
        /// </summary>
        public MethodInfo Target { get; }

        internal CommandInfo(
            ModuleInfo? module, MethodInfo method, string[] aliases, BuildOptions options)
        {
            IsQueryable = true;
            IsDelegate = false;

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

            IsStatic = module == null;

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
            if (IsStatic) // the first argument of the static member will be the context, it takes no instance for the invocation.
            {
                return Target.Invoke(null, [context, .. args]);
            }

            return Target.Invoke(context, args);
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
