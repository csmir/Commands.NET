Commands.NET implements an API specifically for testing, allowing the user to evaluate whether commands will succeed or fail under certain scenarios. 
This is useful for ensuring that commands are functioning as expected, and for debugging issues with commands.

- [Creating Tests](#creating-tests)
- [Testing Commands](#testing-commands)

## Creating Tests

Tests are defined in one of two ways: By creating a new `TestProvider`, or by marking `Test` on modular command methods.

### TestProvider

Alongside other components within Commands.NET, a creation pattern is used to create a new `TestProvider`. This pattern is used to ensure that the provider is correctly initialized.
```cs
var provider = TestProvider.Create(TestResultType.Success, "arguments");
```

### Test Attribute

Tests can be defined by marking a method with the `Test` attribute. This attribute is used to define the test, and will be used by the `TestRunner` to execute the test.

```cs
// Because an argument too many is provided here, the test will fail on Match, which is what the test expects to fail at, so it will succeed.
[Name("testcommand")]
[Test(ShouldEvaluateTo = TestResultType.MatchFailure, Arguments = "arguments")]
public void TestMethod()
{

}
```

## Testing Commands

Collections of commands can be tested in bulk using `TestRunner`. The class is initialized using a creation pattern:

```cs
var runner = TestRunner<TestCallerContext>.Create()
```

This will create a new instance of `TestRunner<TestCallerContext>`, which will be used to test commands. 
The type parameter `TestCallerContext` is a class that implements `ICallerContext`. This interface is used to provide context to the commands being tested.

When testing commands, some processing might be required to provide a clear overview of the execution. Two events are available to implement which will be notified after test execution:

```cs
// Ran when a test is completed.
runner.TestCompleted += (result) => { /* handle result */ };

// Ran when a test fails.
runner.TestFailed += (result) => { /* handle result */ };
```

Now, the runner can be started and awaited, running all available tests made available to it.

```cs
await runner.Run();
```

When this method completes, all tests have been executed. In order to evaluate whether all results ran, the `CountCompleted` value can be compared against `Count.`

```cs
if (runner.CountCompleted == runner.Count)
{
	// All tests ran successfully.
}
```

> [!NOTE]
> To ensure that the context is not shared between commands, `TestRunner<T>` will create a new instance of `T:ICallerContext` for every available test. 
> Therefore, the context type is constrained to `new()`.