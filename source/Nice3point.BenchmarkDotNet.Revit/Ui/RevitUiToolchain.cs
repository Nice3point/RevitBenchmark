using System.Reflection;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Validators;
#if NET
using System.Runtime.Versioning;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
#else
using BenchmarkDotNet.Toolchains.CsProj;
#endif

namespace Nice3point.BenchmarkDotNet.Revit.Ui;

/// <summary>
///     Builds a benchmark with the default toolchain of the current runtime and executes it inside the Revit user interface application.
/// </summary>
/// <remarks>
///     The validation rejects the job settings that apply to a benchmark process BenchmarkDotNet starts, and the asynchronous benchmarks.
///     The benchmark runs synchronously on the Revit thread, and an awaited Revit event would never complete.
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
            yield return CreateError(benchmarkCase, $"The {characteristic.Id} setting of the job applies to a benchmark process, and a Revit user interface benchmark runs in Revit.exe.");
        }

        if (environment.Gc.HasChanges)
        {
            yield return CreateError(benchmarkCase, "The GC settings of the job apply to a benchmark process, and a Revit user interface benchmark runs in Revit.exe.");
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
            yield return CreateError(benchmarkCase, $"{method!.Name} returns {method.ReturnType.Name}. A Revit user interface benchmark runs synchronously on the Revit thread and supports no asynchronous method.");
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
        // The generated project references the benchmark project, and both target the framework and the platform of the entry assembly.
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
        // Revit 2021 to 2024 run on .NET Framework 4.8, and the custom build configuration of the job requires an MSBuild toolchain.
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
