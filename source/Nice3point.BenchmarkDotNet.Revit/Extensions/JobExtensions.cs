using System.Reflection;
using BenchmarkDotNet.Jobs;

// ReSharper disable once CheckNamespace
namespace Nice3point.BenchmarkDotNet.Revit;

/// <summary>
///     Provides extension methods for <see cref="Job" /> to select the build configuration of a benchmark.
/// </summary>
public static class JobExtensions
{
    /// <summary>
    ///     Defines extensions for <see cref="Job" />.
    /// </summary>
    /// <param name="job">The <see cref="Job" /> to configure.</param>
    extension(Job job)
    {
        /// <summary>
        ///     Configures the job to build the benchmark in the build configuration of the entry assembly.
        /// </summary>
        /// <returns>The <see cref="Job" /> with the build configuration of the entry assembly.</returns>
        /// <remarks>
        ///     The job builds in the <c>Release</c> configuration when the entry assembly declares no <see cref="AssemblyConfigurationAttribute" />.
        /// </remarks>
        public Job WithCurrentConfiguration()
        {
            var configuration = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyConfigurationAttribute>()?
                .Configuration ?? "Release";

            return job.WithCustomBuildConfiguration(configuration);
        }
    }
}
