using Commands.Conditions;
using Commands.Helpers;
using System.Diagnostics;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class CommandInfo : ISearchable, IArgumentBucket
    {
        /// <inheritdoc />
        public IInvoker Invoker { get; }

        /// <inheritdoc />
        public string? Name { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public bool IsSearchable { get; }

        /// <inheritdoc />
        public bool IsDefault { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] PreEvaluations { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] PostEvaluations { get; }

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
        public float Priority { get; }

        /// <inheritdoc />
        public ModuleInfo? Module { get; }

        /// <inheritdoc />
        public float Score
        {
            get
            {
                return GetScore();
            }
        }

        internal CommandInfo(StaticInvoker invoker, string[] aliases, bool hasContext, CommandBuilder options)
            : this(null, invoker, aliases, hasContext, options)
        {
        }

        internal CommandInfo(DelegateInvoker invoker, string[] aliases, bool hasContext, CommandBuilder options)
            : this(null, invoker, aliases, hasContext, options)
        {
        }

        internal CommandInfo(
            ModuleInfo? module, IInvoker invoker, string[] aliases, bool hasContext, CommandBuilder options)
        {
            IsSearchable = true;

            var attributes = invoker.Target.GetAttributes(true);

            var parameters = invoker.Target.GetArguments(hasContext, options);

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

            Attributes = attributes.ToArray();

            PreEvaluations = ConditionEvaluator.CreateEvaluators(attributes.CastWhere<IPreExecutionCondition>()).ToArray();
            PostEvaluations = ConditionEvaluator.CreateEvaluators(attributes.CastWhere<IPostExecutionCondition>()).ToArray();

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

        /// <summary>
        ///     Gets the first attribute of the specified type set on this command, if it exists.
        /// </summary>
        /// <typeparam name="T">The attribute type to filter by.</typeparam>
        /// <returns>An attribute of the type <typeparamref name="T"/> if it exists, otherwise <see langword="null"/></returns>
        public T? GetAttribute<T>()
            where T : Attribute
        {
            var attribute = Attributes.FirstOrDefault(x => x is T);

            if (attribute is null)
            {
                return default;
            }

            return (T)attribute;
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
