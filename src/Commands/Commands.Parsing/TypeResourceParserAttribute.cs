using Commands.Parsing;

namespace Commands;

/// <summary>
///     An attribute that indicates that the parameter should be parsed from the request body.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public abstract class TypeResourceParserAttribute : TypeParserAttribute, IResourceBinding
{
}
