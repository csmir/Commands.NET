Execution conditions are pre-execution checks that ensure that the command in scope is allowed to be executed and report success. 

Conditions are evaluated using evaluators, which are responsible for executing the conditions and determining the result of the evaluation.
Evaluators such as OR and AND group conditions together, allowing for complex logical operations to be performed on them.

- [Creating a Condition](#creating-a-condition)
- [Applying a Condition](#applying-a-condition)
- [Custom Evaluators](#custom-evaluators)

## Creating a Condition

All execution conditions must derive from `ExecuteConditionAttribute` or `ExecuteConditionAttribute<T>`.
`T` represents an `IEvaluator`, which is the method in which the condition will be evaluated.

Evaluators by default represent logical operations over groups of conditions, such as `ANDEvaluator` and `OREvaluator`, or a custom implementation.

```cs
using Commands.Conditions;

public class CustomConditionAttribute : ExecuteConditionAttribute<ANDEvaluator>
{
	public ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
	{
		// Your condition logic here
	}
}
```

> [!IMPORTANT]
> It is generally recommended to use the `ExecuteConditionAttribute<T>` generic type, where `T` is a condition evaluator that will be used to evaluate the condition.
> While the `ExecuteConditionAttribute` type can be used directly, it is not recommended. 
> This type creates new evaluators through `Activator.CreateInstance`, which is a much more expensive alternative than `new T()`.

## Applying a Condition

```cs
var command = new Command([CustomCondition] () => "Hello world!", "hello");
```

```cs
[Name("command")]
[CustomCondition]
public void Command()
{
}
```

Conditions are applied to the command by adding the attribute to the method or delegate. 
Modules can also be decorated with conditions, which will be applied to all commands and nested modules within the module.

When new commands are created, parent conditions are automatically propagated to children. 
Likewise, when commands are removed from their parents, evaluators are recreated to only include conditions defined on the command itself.

> [!WARNING]
> When adding a group containing commands to a parent, conditions of the new parent are not automatically propagated to child commands.
> If this is desired, consider adding the group to the parent before adding commands to it. 
> Commands that are added to groups will inherit the conditions of the current parent chain, and are only reevaluated when the command itself is mutated in the tree.

## Custom Evaluators

You can create custom evaluators by implementing the `ConditionEvaluator` abstract type, or the `IEvaluator` interface. 
Default evaluators such as `OREvaluator` and `ANDEvaluator` can also be implemented directly to create new groups using the same logic.

```cs
using Commands.Conditions;

public class CustomEvaluator : ConditionEvaluator
{
	public CustomEvaluator()
	{
		// By specifying the maximum number of conditions that can be evaluated, 
		// you can control how many conditions are allowed in this evaluator.
		MaximumAllowedConditions = 1;

		// Sets the order in which the evaluator is ran respective of other evaluators present for the target command.
		// For example, you might want to run authorisation conditions before context requirement conditions.
		Order = -5; 
	}

	public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
	{
		// Your custom evaluation logic here
	}
}
```
> [!IMPORTANT]
> It is important to ensure that when a custom evaluator is created and a constructor is defined, 
> that it must be public and parameterless to be implemented into your `ExecuteConditionAttribute`

You can then use your custom evaluator in your previously defined custom condition:

```cs
public class CustomConditionAttribute : ExecuteConditionAttribute<CustomEvaluator> // <--
{
	// ...
}
```

If multiple `CustomConditionAttribute` definitions were to exist in the whole condition collection for a specific command, 
it will throw an exception at runtime for exceeding the maximum allowed conditions within the given evaluator (being 1).