using System.Reflection;
using BenchmarkDotNet.Jobs;

// ReSharper disable once CheckNamespace
namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
/// Provides extension methods for configuring jobs in BenchmarkDotNet.
/// </summary>
public static class JobExtensions
{
    /// <param name="job">The job to configure.</param>
    extension(Job job)
    {
        /// <summary>
        /// Configures the job to run with the build configuration of the entry assembly.
        /// </summary>
        public Job WithCurrentConfiguration()
        {
            var configuration = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyConfigurationAttribute>()?
                .Configuration ?? "Release";

            return job.WithCustomBuildConfiguration(configuration);
        }
    }
}