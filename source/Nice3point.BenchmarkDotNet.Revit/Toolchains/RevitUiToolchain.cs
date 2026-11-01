using System.Reflection;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Validators;
using BenchmarkDotNet.Toolchains.CsProj;
#if NET
using System.Runtime.Versioning;
using BenchmarkDotNet.Toolchains.DotNetCli;
#endif

namespace Nice3point.BenchmarkDotNet.Revit.Toolchains;

/// <summary>
///     Represents the toolchain that builds a benchmark with the MSBuild toolchain of the current runtime and executes it inside Revit.
/// </summary>
/// <remarks>
///     The benchmark runs synchronously on the Revit thread.
///     The validation rejects asynchronous benchmark methods.
///     The validation rejects the job settings of a benchmark process: the runtime, the platform, the JIT, the GC settings, and the environment variables.
/// </remarks>
internal sealed class RevitUiToolchain : IToolchain
{
    private readonly IToolchain _buildToolchain = CreateBuildToolchain();

    private RevitUiToolchain()
    {
    }

    /// <summary>
    ///     Gets the toolchain instance.
    /// </summary>
    public static RevitUiToolchain Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "RevitUi";

    /// <inheritdoc />
    public IGenerator Generator => _buildToolchain.Generator;

    /// <inheritdoc />
    public IBuilder Builder => _buildToolchain.Builder;

    /// <inheritdoc />
    public IExecutor Executor { get; } = new RevitUiExecutor();

    /// <inheritdoc />
    public bool IsInProcess => false;

    /// <inheritdoc />
    public IEnumerable<ValidationError> Validate(BenchmarkCase benchmarkCase, IResolver resolver)
    {
        foreach (var error in _buildToolchain.Validate(benchmarkCase, resolver))
        {
            yield return error;
        }

        var environment = benchmarkCase.Job.Environment;
        Characteristic[] processCharacteristics =
        [
            EnvironmentMode.RuntimeCharacteristic,
            EnvironmentMode.PlatformCharacteristic,
            EnvironmentMode.JitCharacteristic,
            EnvironmentMode.EnvironmentVariablesCharacteristic
        ];

        foreach (var characteristic in processCharacteristics.Where(environment.HasValue))
        {
            yield return CreateError(benchmarkCase, $"UI benchmarks run in the Revit process and ignore the {characteristic.Id} setting of the job. Remove the {characteristic.Id} setting from the job.");
        }

        if (environment.Gc.HasChanges)
        {
            yield return CreateError(benchmarkCase, "UI benchmarks run in the Revit process and ignore the GC settings of the job. Remove the GC settings from the job.");
        }

        var descriptor = benchmarkCase.Descriptor;
        MethodInfo?[] methods =
        [
            descriptor.WorkloadMethod,
            descriptor.GlobalSetupMethod,
            descriptor.GlobalCleanupMethod,
            descriptor.IterationSetupMethod,
            descriptor.IterationCleanupMethod
        ];

        foreach (var method in methods.Where(IsAwaitable))
        {
            yield return CreateError(benchmarkCase, $"UI benchmarks run synchronously on the Revit thread. Make {method!.Name} synchronous.");
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }

    private static IToolchain CreateBuildToolchain()
    {
#if NET
        // The generated project targets the framework and the platform of the benchmark project.
        var entryAssembly = Assembly.GetEntryAssembly();
        var frameworkVersion = entryAssembly?.GetCustomAttribute<TargetFrameworkAttribute>() is { } targetFramework
            ? new FrameworkName(targetFramework.FrameworkName).Version
            : new Version(Environment.Version.Major, 0);

        var targetFrameworkMoniker = $"net{frameworkVersion.Major}.{frameworkVersion.Minor}";
        if (entryAssembly?.GetCustomAttribute<TargetPlatformAttribute>() is { } targetPlatform)
        {
            targetFrameworkMoniker = $"{targetFrameworkMoniker}-{targetPlatform.PlatformName}";
        }

        return CsProjCoreToolchain.From(new NetCoreAppSettings(targetFrameworkMoniker, null, $".NET {frameworkVersion.Major}.{frameworkVersion.Minor}"));
#else
        // The Roslyn toolchain, the default on .NET Framework, rejects the build configuration of the job.
        return CsProjClassicNetToolchain.Net48;
#endif
    }

    [Pure]
    private static bool IsAwaitable(MethodInfo? method)
    {
        if (method is null)
        {
            return false;
        }

        var returnType = method.ReturnType;
        return typeof(Task).IsAssignableFrom(returnType) ||
               returnType == typeof(ValueTask) ||
               returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>);
    }

    [Pure]
    private static ValidationError CreateError(BenchmarkCase benchmarkCase, string message)
    {
        return new ValidationError(true, message, benchmarkCase);
    }
}
