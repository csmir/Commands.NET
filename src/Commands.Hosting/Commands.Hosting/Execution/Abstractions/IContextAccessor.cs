﻿namespace Commands.Hosting;

/// <summary>
///     Provides a mechanism for accessing the <see cref="IContext"/> of the current execution pipeline.
/// </summary>
/// <typeparam name="TContext">The implementation of <see cref="IContext"/> this accessor should return. If the current context is no implementation of <typeparamref name="TContext"/>, getting <see cref="Context"/> will throw an exception.</typeparam>
public interface IContextAccessor<out TContext>
    where TContext : IContext
{
    /// <summary>
    ///     Gets the <see cref="IContext"/> of the current execution pipeline.
    /// </summary>
    public TContext Context { get; }
}
