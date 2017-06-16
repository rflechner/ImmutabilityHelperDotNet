``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3318397 Hz, Resolution=301.3503 ns, Timer=TSC
dotnet cli version=1.0.4
  [Host]     : .NET Core 4.6.25211.01, 64bit RyuJITDEBUG [AttachedDebugger]
  DefaultJob : .NET Core 4.6.25211.01, 64bit RyuJIT


```
 |     Method |        Mean |      Error |     StdDev |
 |----------- |------------:|-----------:|-----------:|
 | ManualCopy |    19.57 ns |  0.4268 ns |  0.4192 ns |
 | HelperCopy | 4,799.53 ns | 41.7243 ns | 39.0290 ns |
