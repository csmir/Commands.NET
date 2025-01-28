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
// Because an argument too many is provided here, the test will fail on Match, which is what the test expects to fail at, so it will complete succesfully.
[Name("testcommand")]
[Test(ShouldEvaluateTo = TestResultType.MatchFailure, Arguments = "arguments")]
public void TestMethod()
{

}
```

## Testing Commands

Collections of commands can be tested in bulk using `TestRunner`. The class can be initialized using a functional pattern:

```cs
var runner = TestRunner.From(manager.GetCommands().ToArray()).Create();
```

This will create a new instance of `TestRunner`, which will be used to test commands. 
The runner can be started and awaited, running all available tests made available to it:

```cs
var results = await runner.Run((input) => new TestContext(input));
```

When this method completes, all tests have been executed. 
In order to evaluate whether all results ran, `results.Count(x => x.Success)` can be compared against `runner.Count`.

```cs
if (results.Count(x => x.Success) == runner.Count)
{
	// All tests ran successfully.
}
```

> [!NOTE]
> To ensure that the context is not shared between commands, `TestRunner` will create a new instance of `ICallerContext` using the provided delegate for every available test.