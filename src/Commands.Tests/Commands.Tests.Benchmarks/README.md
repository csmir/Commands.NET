# Commands.NET Benchmarks

Benchmarks are ran for every release to ensure the library is performing as expected. 

The following benchmarks are ran on the latest version of the library, using the latest version of the .NET SDK and BenchmarkDotNet.

### Last Benchmark: 1/22/2025

- **CreateArguments**: Parsing provided string input into processable arguments.
- **FindCommands**: Finding the command to execute based on the provided arguments.
- **RunCommand**: Parsing, Finding and running the command, start to finish.
- **RunCommandAsync**: Parsing, Finding and running the command asynchronously, start to finish.

# Intel® Core™ i7-1255U

| Method          | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------------- |----------:|---------:|---------:|-------:|----------:|
| CreateArguments |  57.85 ns | 0.657 ns | 0.582 ns | 0.0535 |     336 B |
| FindCommands    |  41.74 ns | 0.248 ns | 0.232 ns | 0.0140 |      88 B |
| RunCommand      | 174.23 ns | 1.823 ns | 1.705 ns | 0.0918 |     576 B |
| RunCommandAsync | 174.77 ns | 1.127 ns | 0.880 ns | 0.0918 |     576 B |

> [!NOTE]
> Allocation for running commands is a combination of the argument creation, and search process. 
> On itself, it allocates 156 bytes for actual execution.

# Intel® Core™ Ultra 7 155H 

| Method          | Mean      | Error    | StdDev   | Gen0   | Allocated |
|---------------- |----------:|---------:|---------:|-------:|----------:|
| CreateArguments |  53.25 ns | 0.499 ns | 0.417 ns | 0.0268 |     336 B |
| FindCommands    |  42.23 ns | 0.467 ns | 0.414 ns | 0.0070 |      88 B |
| RunCommand      | 172.21 ns | 0.726 ns | 0.679 ns | 0.0458 |     576 B |
| RunCommandAsync | 168.47 ns | 1.027 ns | 0.961 ns | 0.0458 |     576 B |

*This chart can be reproduced at any time by running this benchmark project.*