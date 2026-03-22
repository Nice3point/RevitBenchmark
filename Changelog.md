# 2027.0.0-preview.2.20260203

This release adds support for Revit 2027, benchmarking for different languages and custom Revit installation path.

## Localization support

BenchmarkDotNet initializes Revit with the `English - United States` language. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., Program.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;
    
    [assembly: RevitLanguage("ENU")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitLanguageAttribute">
            <_Parameter1>ENU</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
    ```

The `RevitLanguage` attribute accepts a [language](https://help.autodesk.com/view/RVT/2026/ENU/?guid=GUID-BD09C1B4-5520-475D-BE7E-773642EEBD6C) name (e.g., "English - United States"), code (e.g., "ENU")
or [LanguageType](https://www.revitapidocs.com/2026/dfda33cf-cbff-9fde-6672-38402e87510f.htm) enum value (e.g., "English_GB" or "15").

## Custom Revit installation path

TUnit initializes Revit from `C:\Program Files\Autodesk\Revit {version}` installation path. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., TestsConfiguration.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;
    
    [assembly: RevitInstallationPath("D:\Autodesk\Revit Preview")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitInstallationPathAttribute">
            <_Parameter1>D:\Autodesk\Revit $(RevitVersion)</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
    ```

## Breaking changes

- Global hooks `OnSetup`/`OnCleanup` renamed to `OnGlobalSetup`/`OnGlobalCleanup`

# 2026.0.0

Initial release. Enjoy!