Conditions are checks that ensure that the command in scope is allowed to be executed and report success. 

- [Creating your Condition](#creating-your-condition)
- [Using your Condition](#using-your-condition)
- [Logical operations](#logical-operations)

## Creating your Condition

All preconditions inherit `ExecuteConditionAttribute<TEvaluator>`, which in turn inherits `Attribute`. 
To start creating your own condition, you have to inherit `ExecuteConditionAttribute<TEvaluator>` on a class:

```cs
using Commands;

namespace Commands.Samples;

public class RequireOperatingSystemAttribute : ExecuteConditionAttribute<ANDEvaluator>
{
    public override ValueTask<CheckResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
    }
}
```

With this class defined, and the method that will operate the evaluation being implemented, we can now write our code which defines the succession and failure conditions.

First of all, the restriction must be compared against. 
To define this, we will implement a constructor parameter, automatically resolved as an attribute parameter by the IDE or code editor:

```cs
using Commands;

namespace Commands.Samples;

public class RequireOperatingSystemAttribute(PlatformID platform) : ExecuteConditionAttribute<ANDEvaluator>
{
    public PlatformID Platform { get; } = platform;

    public override ValueTask<CheckResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
    }
}
```

After this has been defined, the `Platform` property can be used to evaluate the current operating system against. 
Our focus goes to the `EvaluateAsync` method, which will run this check.

```cs
...
    if (Environment.OSVersion.Platform == Platform)
        return Success();

    return Error("The platform does not support this operation.");
...
```

That's it. With this done, we can look towards the application of our created condition.

> [!TIP]
> `ExecuteCondition` is a 1:1 counterpart of the attribute-based implementation, serving as a variant for the runtime creation pattern. 
> When using this pattern, `DelegateExecuteCondition` is also available.

## Using your Condition

After you have written your condition, it is time to use it. Let's define a command that depends on the operating system to run.

```cs
[Command("copy")]
public void Copy([Remainder] string toCopy)
{
    Process clipboardExecutable = new()
    {
        StartInfo = new ProcessStartInfo
        {
            RedirectStandardInput = true,
            FileName = "clip",
        }
    };
    clipboardExecutable.Start();

    clipboardExecutable.StandardInput.Write(toCopy);
    clipboardExecutable.StandardInput.Close();

    Console.Writeline("Succesfully copied the content to your clipboard.");
}
```

This method will use the Windows clip executable to copy a string onto your clipboard. 
Though, this clipboard approach does not work on MacOS or Linux, so we have to make sure the command is executed on windows.

To do this, all we have to do is add `[RequireOperatingSystem(PlatformID.Win32NT)]` above the method, like so:

```cs
[Command("copy")]
[RequireOperatingSystem(PlatformID.Win32NT)]
public void Copy([Remainder] string toCopy)
...
```

The conditions is now defined on this command, and will be called when this command is triggered. 
If the platform you run it on is indeed not Windows, it will fail.

## Logical operations

Conditions can be combined with logical operations. This can be done by adding multiple conditions to a command, and defining the logical operation in the condition you defined. There are two standard types:

- `OREvaluator`: This will return success if any of the conditions implementing `OREvaluator` succeed.

- `ANDEvaluator`: This will return success if all of the conditions implementing `ANDEvaluator` succeed.