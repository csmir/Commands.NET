Conditions are checks that ensure that the command in scope is allowed to be executed and report success. 

- [Creating a Condition](#creating-a-condition)
- [Applying a Condition](#applying-a-condition)

## Creating a Condition

Conditions are based on evaluators. 
`ConditionEvaluator` implementations like `OREvaluator` and `ANDEvaluator` use logical operations to evaluate conditions, grouping them together per implementation of `ConditionEvaluator`.

For the following examples, `ANDEvaluator` will be used. This means that all conditions must return success for the command to be executed.

### Functional Pattern

```cs
var condition = ExecuteCondition.For<ANDEvaluator>().AddDelegate(ctx, cmd, services) => ...);
```

The creation pattern handles conditions as `ValueTask<ConditionResult>` where `ConditionResult.FromError()` or `ConditionResult.FromSuccess()` can be used to return the result. 
`ConditionResult` implicitly converts to `ValueTask<T>`.

### Declarative Pattern

```cs
using Commands.Conditions;

public class CustomCondition : ExecuteCondition<ANDCondition>
{
	public ValueTask<ConditionResult> Evaluate(ICallerContext context, Command command, IServiceProvider services)
	{
		// Your condition logic here
	}
}
```

The declarative pattern implements shorthand access to `ConditionResult` using the exposed `Error()` and `Success()` methods.

### Attribute Pattern

```cs
using Commands.Conditions;

public class CustomConditionAttribute : ExecuteConditionAttribute<ANDCondition>
{
	public ValueTask<ConditionResult> Evaluate(ICallerContext context, Command command, IServiceProvider services)
	{
		// Your condition logic here
	}
}
```

The attribute pattern is exclusive to the module-based command system. 
This pattern writes similar to `ExecuteCondition` implementations, also allowing shorthand calls to be used.

## Applying a Condition

### Functional Pattern & Declarative Pattern

```cs
var command = Command.From(() => { }, "name").AddCondition(condition);
```
```cs
var group = CommandGroup.From("name").AddCondition(new CustomCondition());
```

Conditions exposed to `CommandGroup` are passed to every `Command` and `CommandGroup` added to it.

### Attribute Pattern

```cs
[Name("command")]
[CustomCondition]
public void Command()
{
}
```

Conditions are applied to the command by adding the attribute to the method. 
Modules can also be decorated with conditions, which will be applied to all commands and nested modules within the module.
