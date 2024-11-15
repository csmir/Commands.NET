using Commands.Conditions;
using Commands.Helpers;
using System.Diagnostics;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a command module, hosting zero-or-more commands.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class ModuleInfo : ISearchable
    {
        /// <summary>
        ///     Gets an array containing nested modules or commands inside this module.
        /// </summary>
        public IReadOnlySet<ISearchable> Components { get; }

        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type Type { get; }

        /// <inheritdoc />
        public IInvoker Invoker { get; }

        /// <inheritdoc />
        public string? Name { get; }

        /// <inheritdoc />
        public string FullName { get; }

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

        internal ModuleInfo(
            Type type, ModuleInfo? root, string[] aliases, CommandConfiguration options)
        {
            var attributes = type.GetAttributes(true);

            Module = root;
            Type = type;

            Attributes = attributes.ToArray();

            PreEvaluations = ConditionEvaluator.CreateEvaluators(attributes.CastWhere<IPreExecutionCondition>()).ToArray();
            PostEvaluations = ConditionEvaluator.CreateEvaluators(attributes.CastWhere<IPostExecutionCondition>()).ToArray();

            Priority = attributes.SelectFirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

            Aliases = aliases;

            if (aliases.Length > 0)
            {
                IsSearchable = true;
                Name = aliases[0];
            }
            else
            {
                IsSearchable = false;
                Name = null;
            }

            FullName = $"{(Module != null && Module.Name != null ? $"{Module.FullName} " : "")}{Name}";

            IsDefault = false;

            Invoker = new ConstructorInvoker(type, options);

            Components = ReflectionUtilities.GetComponents(this, options)
                .OrderByDescending(x => x.Score)
                .ToHashSet();
        }

        /// <inheritdoc />
        public float GetScore()
        {
            if (Components.Count == 0)
                return 0.0f;

            var score = 1.0f;

            foreach (var component in Components)
            {
                score += component.GetScore();
            }

            if (Name != Type.Name)
                score += 1.0f;

            score += Priority;

            return score;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{(Module != null ? $"{Module}." : "")}{(Name != null ? $"{Type.Name}['{Name}']" : $"{Type.Name}")}";
        }
    }
}
