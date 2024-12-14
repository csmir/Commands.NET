using Commands.Conditions;
using System.Diagnostics;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class CommandInfo : IComponent, IArgumentBucket
    {
        private readonly Guid __id = Guid.NewGuid();

        /// <inheritdoc />
        public IInvoker Invoker { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] PreEvaluations { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] PostEvaluations { get; }

        /// <inheritdoc />
        public IArgument[] Arguments { get; }

        /// <inheritdoc />
        public bool HasRemainder { get; }

        /// <inheritdoc />
        public int MinLength { get; }

        /// <inheritdoc />
        public int MaxLength { get; }

        /// <inheritdoc />
        public float Priority { get; }

        /// <inheritdoc />
        public ModuleInfo? Parent { get; }

        /// <inheritdoc />
        public string? Name
            => Aliases.Length > 0 ? Aliases[0] : null;

        /// <inheritdoc />
        public string FullName
            => $"{(Parent != null && Parent.Name != null ? $"{Parent.FullName} " : "")}{Name}";

        /// <inheritdoc />
        public float Score
            => GetScore();

        /// <inheritdoc />
        public bool IsRuntimeComponent
            => Parent == null;

        /// <inheritdoc />
        public bool IsSearchable
            => true;

        /// <inheritdoc />
        public bool IsDefault
            => Aliases.Length == 0;

        /// <inheritdoc />
        public bool HasArguments
            => Arguments.Length > 0;

        internal CommandInfo(StaticInvoker invoker, string[] aliases, bool hasContext, BuildConfiguration options)
            : this(null, invoker, aliases, hasContext, options)
        {

        }

        internal CommandInfo(DelegateInvoker invoker, IExecuteCondition[] conditions, string[] aliases, bool hasContext, BuildConfiguration options)
            : this(null, invoker, aliases, hasContext, options)
        {
            PreEvaluations = ConditionEvaluator.CreateEvaluators(conditions.OfType<IPreExecutionCondition>()).ToArray();
            PostEvaluations = ConditionEvaluator.CreateEvaluators(conditions.OfType<IPostExecutionCondition>()).ToArray();
        }

        internal CommandInfo(
            ModuleInfo? module, IInvoker invoker, string[] aliases, bool hasContext, BuildConfiguration options)
        {
            var attributes = invoker.Target.GetAttributes(true).Concat(module?.Attributes ?? []).Distinct();

            var parameters = invoker.Target.GetArguments(hasContext, options);

            var (minLength, maxLength) = parameters.GetLength();

            Aliases = aliases;

            if (parameters.Any(x => x.IsRemainder))
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];

                    if (parameter.IsRemainder && i != parameters.Length - 1)
                        throw new InvalidOperationException($"{nameof(RemainderAttribute)} can only exist on the last parameter of a command signature. Command: {FullName}");
                }
            }

            Priority = attributes.GetAttribute<PriorityAttribute>()?.Priority ?? 0;

            Invoker = invoker;
            Parent = module;

            Attributes = attributes.ToArray();

            PreEvaluations = ConditionEvaluator.CreateEvaluators(attributes.OfType<IPreExecutionCondition>()).ToArray();
            PostEvaluations = ConditionEvaluator.CreateEvaluators(attributes.OfType<IPostExecutionCondition>()).ToArray();

            Arguments = parameters;
            HasRemainder = parameters.Any(x => x.IsRemainder);

            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <inheritdoc />
        public float GetScore()
        {
            var score = 1.0f;

            foreach (var argument in Arguments)
                score += argument.GetScore();

            score += Priority;

            return score;
        }

        /// <inheritdoc />
        public override string ToString()
            => ToString(true);

        /// <inheritdoc cref="ToString()"/>
        /// <param name="withModuleInfo">Defines if the module information should be appended on the command level.</param>
        public string ToString(bool withModuleInfo)
            => $"{(withModuleInfo ? $"{Parent}." : "")}{Invoker.Target.Name}{(Name != null ? $"['{Name}']" : "")}({string.Join<IArgument>(", ", Arguments)})";

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is CommandInfo info && info.__id == __id;

        /// <inheritdoc />
        public override int GetHashCode()
            => __id.GetHashCode();
    }
}
