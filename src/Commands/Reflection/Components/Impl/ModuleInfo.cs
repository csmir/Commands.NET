using Commands.Conditions;
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
        public HashSet<ISearchable> Components { get; }

        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type? Type { get; }

        /// <inheritdoc />
        public IInvoker? Invoker { get; }

        /// <inheritdoc />
        public string? Name { get; }

        /// <inheritdoc />
        public string FullName { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

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
            => GetScore();

        /// <inheritdoc />
        public bool IsRuntimeComponent
            => Type == null;

        /// <inheritdoc />
        public bool IsSearchable
            => Aliases.Length > 0;

        /// <inheritdoc />
        public bool IsDefault
            => false;

        internal ModuleInfo(
            Type type, ModuleInfo? root, string[] aliases, CommandConfiguration options)
        {
            Module = root;
            Type = type;

            var attributes = type.GetAttributes(true).Concat(root?.Attributes ?? []).Distinct();

            Attributes = attributes.ToArray();

            PreEvaluations = ConditionEvaluator.CreateEvaluators(attributes.OfType<IPreExecutionCondition>()).ToArray();
            PostEvaluations = ConditionEvaluator.CreateEvaluators(attributes.OfType<IPostExecutionCondition>()).ToArray();

            Priority = attributes.GetAttribute<PriorityAttribute>()?.Priority ?? 0;

            Aliases = aliases;

            if (aliases.Length > 0)
                Name = aliases[0];
            else
                Name = null;

            FullName = $"{(Module != null && Module.Name != null ? $"{Module.FullName} " : "")}{Name}";

            Invoker = new ConstructorInvoker(type);

            Components = [.. ReflectionUtilities.GetComponents(this, options).OrderByDescending(x => x.Score)];
        }

        internal ModuleInfo(
            ModuleInfo? root, string[] aliases)
        {
            Module = root;

            Components = [];
            Attributes = [];
            PreEvaluations = [];
            PostEvaluations = [];

            Aliases = aliases;
            Name = aliases[0];

            FullName = $"{(Module != null && Module.Name != null ? $"{Module.FullName} " : "")}{Name}";
        }

        /// <inheritdoc />
        public float GetScore()
        {
            if (Components.Count == 0)
                return 0.0f;

            var score = 1.0f;

            foreach (var component in Components)
                score += component.GetScore();

            if (Name != Type?.Name)
                score += 1.0f;

            score += Priority;

            return score;
        }

        /// <summary>
        ///     Sorts all items in the module based on their score, which should be called when new components are added at runtime after the module has already been initialized.
        /// </summary>
        public void SortScores()
        {
            var copy = Components.ToArray();

            copy.OrderByDescending(x => x.Score);

            Components.Clear();

            foreach (var component in copy)
                Components.Add(component);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{(Module != null ? $"{Module}." : "")}{(Name != null ? $"{Type?.Name}['{Name}']" : $"{Type?.Name}")}";
        }
    }
}
