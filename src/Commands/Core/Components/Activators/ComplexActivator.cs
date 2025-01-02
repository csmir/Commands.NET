﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Commands;

/// <summary>
///     Represents an activator that can create an instance of a complex type, being a parameter marked with <see cref="ComplexAttribute"/>.
/// </summary>
public sealed class ComplexActivator : IActivator
{
    private readonly ConstructorInfo _ctor;

    /// <inheritdoc />
    public MethodBase Target
        => _ctor;

    internal ComplexActivator(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type)
    {
        var ctors = type.GetAvailableConstructors();

        _ctor = ctors.First();
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, CommandInfo? command, object?[] args, IComponentTree? tree, CommandOptions options) where T : ICallerContext
        => _ctor.Invoke(args);
}