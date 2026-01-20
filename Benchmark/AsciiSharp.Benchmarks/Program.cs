using BenchmarkDotNet.Running;

// ベンチマーククラスが追加されたら、ここで実行する
// 例: BenchmarkRunner.Run<ParserBenchmarks>();
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
