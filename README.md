# Cake.BuildSystems.Module

A simple Cake module to enhance running from a hosted CI environment

This module will introduce a number of features for running in hosted CI build environments to tightly integrate with the host environment/tools. These modules require **no changes to build scripts** and instead rely on your Cake script's standard aliases and lifecycle events to integrate your script into the environment.

## Build Systems

Currently this module supports:

### Azure Pipelines

> This applies to TFS, Azure Pipelines, and Azure DevOps Server

- Individual timeline records for each task
- Percentage reporting on build progress
- Integrates `Warning` and `Error` logging aliases with the Build Issues summary
- Includes a Cake Build Summary widget on the build summary page, with an emoji per task status
- Colour codes the build summary table in the build log, the way a regular terminal does

### TeamCity

> Tested with TeamCity 10

- Build Logs are separated (and nested) for each executed task
- Current/ongoing build status is updated to currently running task
- `Error` logging aliases are highlighted in build log output

### MyGet

> Supports the MyGet Build Service

- Task records are added to build logs
- Includes a task summary in the build log
- Integrates `Warning`, `Error` and `Fatal` logging aliases with the build log and report 

### Travis CI

> This module is affected by a bug in Travis CI's Linux image (see [travis-ci/travis-ci#7262](https://github.com/travis-ci/travis-ci/issues/7262))

- Log folding for the Cake build and for individual tasks

### GitLab CI

- Add colored output for Cake log messages in the GitLab pipeline output window

## Usage

Each build system's functionality resides in its own module, with `Cake.Module.Shared` used for shared types. Each module will conditionally register itself, meaning they will only be loaded in their respective CI environments. This means all modules can be deployed with a single codebase without interference.

## Configuration

These modules read their settings from [Cake's configuration](https://cakebuild.net/docs/running-builds/configuration/), so each of the settings below can be given in three ways. The `BuildSystems_` prefix in the table is a section in the configuration file:

```ini
; cake.config
[BuildSystems]
HumanizedTimespans=true
```

As an environment variable, prefixed with `CAKE_`:

```bash
CAKE_BUILDSYSTEMS_HUMANIZEDTIMESPANS=true
```

Or as an argument to the build:

```bash
dotnet cake --buildsystems_humanizedtimespans=true
```

| Setting | Default | Description |
|:--------|:--------|:------------|
| `BuildSystems_HumanizedTimespans` | `false` | Report task durations the way a person reads a stopwatch instead of with full `TimeSpan` precision. See [Humanized timespans](#humanized-timespans). |
| `BuildSystems_FailOnFatal` | `false` | MyGet only. Report a `Fatal` log message as a build problem rather than as a failed message. |

### Humanized timespans

Durations are reported with full `TimeSpan` precision by default, exactly as Cake itself reports them. Setting `BuildSystems_HumanizedTimespans` to `true` shortens them:

```
Task                           Duration  Status
----------------------------------------------------
Compile                            2:01  Succeeded
UpdateBuildInformation            < 1 s  Succeeded
Publish                                  Skipped
----------------------------------------------------
Total:                            20:39
```

Anything under a second becomes `< 1 s`, an hour or more gains an hours part as `1:02:03`, and a task that never ran shows no duration at all.

This applies to every build summary these modules render themselves, so to Azure Pipelines, GitHub Actions and MyGet. TeamCity, Travis CI and GitLab CI register no report printer of their own - their summary is the one Cake prints - so the setting has no effect for them.

## Installation

### Using the pre-processor directives

Add the next following line to your Cake script:

```cs
#module nuget:?package=Cake.BuildSystems.Module&version=##see below for note on versioning##
```

Note: The current version can always be taken from [nuget.org](https://nuget.org/packages/Cake.BuildSystems.Module/)).

Currently modules require "bootstrapping", so the first step before running the build is to call `dotnet cake --bootstrap`
(or `.\build.ps1 --bootstrap`)

### Other methods

You can also integrate this module into your own build process, even with a customized `build.ps1`/`build.sh`. As long as the `Cake.BuildSystems.Module` NuGet package is installed into your modules directory ([by default](https://cakebuild.net/docs/running-builds/configuration/default-configuration-values) `./tools/Modules`), `cake.exe` should pick them up when it runs. Note that you can also change your modules directory using the `cake.config` file or passing arguments to `cake.exe` as outlined in [the documentation](https://cakebuild.net/docs/running-builds/configuration/set-configuration-values)).

### Cake.Frosting

While Cake script will load all modules automatically that are present in the modules directory, the same is not the case when using Cake.Frosting.

Before using Cake.Buildsystems.Module you have to reference the NuGet package either by adding `<PackageReference Include="Cake.BuildSystems.Module" Version="##see below for note on versioning##" />` to the `csproj` file of the build project, or by running `dotnet add package cake.buildsystems.module` in the folder of the build project.

To actually make use of the different modules included in Cake.Buildsystems.Module they need to be registered to the `CakeHost`. This can be done by using `ICakeHost.UseModule<TModule>()`. Typically the `CakeHost` is set up in the `Main` method of the build project. All modules included in Cake.Buildsystems.Module can be registered, regardless of the underlying build system, as each modules will only be triggered for the intended build system.

An example that registers all currently existing modules from Cake.Buildsystems.Module:

```csharp
public static int Main(string[] args)
{
    return new CakeHost()
        // Register all modules from Cake.Buildsystems.Module
        .UseModule<AzurePipelinesModule>()
        .UseModule<MyGetModule>()
        .UseModule<TravisCIModule>()
        .UseModule<TeamCityModule>()
        .UseModule<GitLabCIModule>()
        // continue with the "normal" setup of the CakeHost
        .UseContext<BuildContext>()
        .Run(args);
}
```

## Versioning

Note that since modules interact with the internals of Cake, they are tied to a specific version of Cake. The version of Cake supported by the particular module version will always be in the Release Notes of the NuGet package (and therefore also on [nuget.org](https://nuget.org/packages/Cake.BuildSystems.Module/)). Make sure to match this version number to the Cake version you're using.

## Building

Running `build.ps1` with the default target will build all system modules into `BuildArtifacts/temp/_PublishedLibraries`. Copy this to `tools/Modules` (you may need to  disable the MD5 check if using the bootstrapper) and run your script.