# Documentation

These rules govern every piece of prose the package ships: XML doc comments, `README.md`, and `CHANGELOG.md`. Each format adds its own rules on top of the shared set.

A public-surface change updates the README, the CHANGELOG, and the affected XML docs in the same commit. Documentation that lags the code is a defect.

## Shared Prose Rules

* **State what, not how.** Describe observable behavior and contract, never the implementation. A summary survives an implementation rewrite unchanged.
* **Plain technical English.** No corporate jargon, no marketing tone.
* **No filler.** Omit obvious statements. State only what a reader cannot infer from the signature.
* **Third-person present indicative.** Write "Opens the connection", not "Opening the connection". No `-ing` verb form for what a member does.
* **One sentence per line.** Break at sentence boundaries, never at a fixed character width.
* **No dashes or semicolons.** Use separate sentences or commas.

## XML Doc Comments

* Document every public member with a `<summary>` that states what it does.
* **`<summary>` describes the member, not its parameters.** Parameters belong in `<param>`, the return value in `<returns>`, and thrown exceptions in `<exception>`. Do not restate the signature in prose.
* For a wrapper over the Revit API or BenchmarkDotNet, mirror the corresponding source summary and document the exceptions the member can throw.
* Add `<remarks>` for a non-trivial constraint, such as when a hook runs in the benchmark lifecycle or a threading requirement.
* Reference another type or member with `<see cref="..."/>` so renames stay tracked.

## README

The README is the consumer's guide to writing a benchmark with the library. Every feature has a copy-pasteable usage example under its matching section, ordered from the first benchmark through application-level and document benchmarks to run configuration. A new feature gains an example under the section it belongs to. A new overload or related member belongs with its primary feature, not a new top-level section. Keep examples copy-pasteable with C# syntax highlighting.

## CHANGELOG

The CHANGELOG records each release under a version heading, most recent first, with a short lead sentence that names what the release adds. Group the details under descriptive subheadings for the notable changes, and call out breaking changes under a dedicated subheading with a migration note. Show the consumer-facing usage for a new capability the same way the README does.
