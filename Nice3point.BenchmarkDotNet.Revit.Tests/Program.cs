using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Nice3point.BenchmarkDotNet.Revit;
using Nice3point.BenchmarkDotNet.Revit.Tests;

var configuration = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(Job.Default.WithCurrentConfiguration());

BenchmarkRunner.Run<RevitCollectorTests>(configuration);