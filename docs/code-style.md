# Code Style

Production C# only. This is a public library, so its style is part of its contract.

## General Principles

* **SOLID and DRY.** One responsibility per type. Extract shared logic rather than duplicate it.
* **Explicit over implicit.** Code is self-explanatory. Avoid hidden behavior and unclear defaults.
* **Base classes for entry points.** A consumer derives from a base class and overrides one hook. Connection and lifecycle code lives in the base class, never in the consumer's benchmark.
* **Nullable safety.** Nullable reference types are enabled solution-wide. Treat every nullability warning as a defect.
* **StyleCop style.** Follow StyleCop conventions for layout, member ordering, and spacing.

## Modern C#

`LangVersion` is `latest`. Reach for the newest feature that expresses the intent directly, and do not hand-roll what the language already provides.

* Extension members through `extension` blocks for API surface added to BenchmarkDotNet types.
* Primary constructors when a type captures state.
* Collection expressions for literals and spans.
* Pattern matching and switch expressions over branching chains.
* Expression-bodied members for simple accessors.
* File-scoped namespaces.

## Comments

Public types and members carry XML doc comments, see [Documentation](./documentation.md). Inside the code, comments are the exception.

* Names and structure carry the meaning. Default to no comment.
* Add one only when the reason cannot be read from the code and a reader could break the code without it, such as a threading constraint or a BenchmarkDotNet requirement that the signature does not reveal.
* A comment explains why, never what. Do not restate the code.

## Attributes

Decorate members with every JetBrains and .NET attribute that carries meaning, so analyzers, the debugger, and callers read the full contract.

* `[PublicAPI]` on every public type.
* `[Pure]` on a read-only method.
* `[Obsolete]` on a deprecated member, kept functional. See the backward-compatibility rules below.
* BenchmarkDotNet lifecycle attributes, such as the global setup and cleanup markers, on the base-class hooks that drive the run.
* `[ModuleInitializer]` on the one-time process setup the injection path requires.

## Naming

* **Clarity first.** Names are descriptive and never abbreviated: `application` not `app`, `document` not `doc`, `configuration` not `config`.
* Follow the Revit API and BenchmarkDotNet naming conventions.
* A consumer override reads as the thing it does, such as a global setup or cleanup hook.
* No single-letter variables except in a short loop or lambda.

## File and Class Structure

* **File-scoped namespaces.** Use the package namespace or a sub-namespace. When a file lives in a subfolder but keeps the flatter package namespace, add `// ReSharper disable once CheckNamespace` above the declaration.
* **Member order:** private fields, constructors, public properties, public methods, protected hooks, private methods.

## Base-Class Hook Pattern

The base class owns the BenchmarkDotNet lifecycle and the Revit connection. It binds the framework hooks to the connection methods and exposes empty virtual hooks the consumer overrides:

```csharp
[GlobalSetup]
public void SessionSetup()
{
    OpenConnection();
    OnGlobalSetup();
}

[GlobalCleanup]
public void SessionCleanup()
{
    OnGlobalCleanup();
    CloseConnection();
}

protected virtual void OnGlobalSetup()
{
}

protected virtual void OnGlobalCleanup()
{
}
```

The consumer overrides `OnGlobalSetup` and `OnGlobalCleanup` to prepare and dispose per-session state. The connection methods stay inside the base class.

## Backward Compatibility

This is a public library with downstream consumers. A breaking change breaks other people's builds.

* Never delete or change an existing public API. To rename or replace a member, mark the old one `[Obsolete]` with a message that names the replacement, and keep it forwarding to the working implementation.
* Never change a method signature or return type. Add a new overload instead.
* Add new parameters only as optional, and only at the end of the list.
* Document every addition, deprecation, and behavior change in the CHANGELOG. See [Documentation](./documentation.md).

## Compilation Directives

* `#if REVIT2024_OR_GREATER` and similar for version-specific Revit APIs.
* `#if NET` or `#if NET8_0_OR_GREATER` for runtime-specific features.
* Apply directives consistently across related members so a type's surface stays coherent per version. See [Revit Best Practices](./revit-best-practices.md).
