``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-6560U CPU 2.20GHz (Skylake), ProcessorCount=4
Frequency=2156249 Hz, Resolution=463.7683 ns, Timer=TSC
dotnet cli version=1.0.4
  [Host]     : .NET Core 4.6.25211.01, 64bit RyuJITDEBUG [AttachedDebugger]
  DefaultJob : .NET Core 4.6.25211.01, 64bit RyuJIT


```
 |     Method |        Mean |       Error |     StdDev |
 |----------- |------------:|------------:|-----------:|
 | ManualCopy |    26.28 ns |   0.6541 ns |   1.592 ns |
 | HelperCopy | 7,206.77 ns | 173.0060 ns | 212.467 ns |
