# Last Benchmark: 3/5/2024

## Summary

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2


| Method         | Mean     | Error    | StdDev   | Gen0   | Allocated |
|--------------- |---------:|---------:|---------:|-------:|----------:|
| RunCommand     | 67.86 ns | 1.237 ns | 1.157 ns | 0.0535 |     336 B |
| RunParametered | 65.13 ns | 0.624 ns | 0.583 ns | 0.0548 |     344 B |
| RunNested      | 64.02 ns | 0.428 ns | 0.380 ns | 0.0548 |     344 B |

## Hints
Outliers
  Program.RunParametered: Default -> 1 outlier  was  detected (65.48 ns)
  Program.RunNested: Default      -> 1 outlier  was  removed (67.07 ns)

## Legends
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Gen0      : GC Generation 0 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 ns      : 1 Nanosecond (0.000000001 sec)

## BenchmarkRunner: End
Run time: 00:00:48 (48.12 sec), executed benchmarks: 3

Global total time: 00:01:03 (63.61 sec), executed benchmarks: 3