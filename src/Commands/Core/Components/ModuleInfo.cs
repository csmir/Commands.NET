﻿using Commands.Conditions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Commands
{
    /// <summary>
    ///     Reveals information about a command module, hosting zero-or-more commands.
    /// </summary>
    [DebuggerDisplay("Count = {Count}, {ToString()}")]
    public sealed class ModuleInfo : ComponentCollection, IComponent
    {
        /// <summary>
        ///     Gets the type of this module.
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        ///     Gets the depth of the module, being how deeply nested it is in the command tree.
        /// </summary>
        public int Depth
            => Parent?.Depth + 1 ?? 1;

        /// <inheritdoc />
        public IActivator? Activator { get; }

        /// <inheritdoc />
        public string[] Aliases { get; }

        /// <inheritdoc />
        public Attribute[] Attributes { get; }

        /// <inheritdoc />
        public ConditionEvaluator[] Conditions { get; }

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
            => Type == null;

        /// <inheritdoc />
        public bool IsSearchable
            => Aliases.Length > 0;

        /// <inheritdoc />
        public bool IsDefault
            => false;

        internal ModuleInfo(
            Type type, ModuleInfo? root, string[] aliases, ComponentConfiguration configuration)
            : base(configuration.GetProperty(ConfigurationPropertyDefinitions.MakeModulesReadonly, false))
        {
            Parent = root;
            Type = type;

            var attributes = type.GetAttributes(true).Concat(root?.Attributes ?? []).Distinct();

            Attributes = attributes.ToArray();

            Conditions = ConditionEvaluator.CreateEvaluators(attributes.OfType<IExecuteCondition>()).ToArray();

            Aliases = aliases;

            Activator = new ModuleActivator(type);

            Push(configuration.GetNestedComponents(this).OrderByDescending(x => x.Score));
        }

        internal ModuleInfo(
            ModuleInfo? root, string[] aliases)
            : base(false)
        {
            Parent = root;

            Attributes = [];
            Conditions = [];

            Aliases = aliases;
        }

        /// <inheritdoc />
        public float GetScore()
        {
            if (Count == 0)
                return 0.0f;

            var score = 1.0f;

            foreach (var component in this)
                score += component.GetScore();

            if (Name != Type?.Name)
                score += 1.0f;

            score += Attributes.GetAttribute<PriorityAttribute>()?.Priority ?? 0;

            return score;
        }

        /// <inheritdoc />
        public int CompareTo(object? obj)
            => obj is IScorable scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

        /// <inheritdoc />
        public bool Equals(IComponent? other)
            => other is ModuleInfo info && ReferenceEquals(this, info);

        /// <inheritdoc />
        public override IEnumerable<SearchResult> Find(ArgumentEnumerator args)
        {
            List<SearchResult> discovered =
            [
                SearchResult.FromError(this)
            ];

            var searchHeight = Depth;

            foreach (var component in this)
            {
                if (component.IsDefault)
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight));

                if (args.TryNext(searchHeight, out var value) && component.Aliases.Contains(value))
                {
                    if (component is ModuleInfo module)
                        discovered.AddRange(module.Find(args));
                    else
                        discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
                }
            }

            return discovered;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{(Parent != null ? $"{Parent}." : "")}{(Name != null ? $"{Type?.Name}['{Name}']" : $"{Type?.Name}")}";

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is ModuleInfo info && ReferenceEquals(this, info);

        /// <inheritdoc />
        public override int GetHashCode()
            => Activator!.Target.GetHashCode();
    }
}
