# Cake.BuildSystems.Module

A simple Cake module to enhance running from a hosted CI environment

This module will introduce a number of features for running in hosted CI build environments to tightly integrate with the host environment/tools. These modules require **no changes to build scripts** and instead rely on your Cake script's standard aliases and lifecycle events to integrate your script into the environment.

## Build Systems

Currently this module supports:

### TF Build

> This applies to TFS and VSTS

- Individual timeline records for each task
- Percentage reporting on build progress
- Integrates `Warning` and `Error` logging aliases with the Build Issues summary
- Includes a Cake Build Summary widget on the build summary page

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

## Usage

Each build system's functionality resides in its own module, with `Cake.Module.Shared` used for shared types. Each module will conditionally register itself, meaning they will only be loaded in their respective CI environments. This means all modules can be deployed with a single codebase without interference.

## Installation

### Using the latest bootstrapper

If you're using the latest bootstrapper example (always available in [this repo](https://github.com/cake-build/resources)), you can simply add a `tools/Modules/packages.config` file with the following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
    <package id="Cake.BuildSystems.Module" version="##see below for note on versioning##" />
</packages>
```

The next time you run the bootstrapper, the modules should be installed.

### Other methods

You can also integrate this module into your own build process, even with a customised `build.ps1`/`build.sh`. As long as the `Cake.BuildSystems.Module` NuGet package is installed into your modules directory ([by default](http://cakebuild.net/docs/fundamentals/default-configuration-values) `./tools/Modules`), `cake.exe` should pick them up when it runs. Note that you can also change your modules directory using the `cake.config` file or passing arguments to `cake.exe` as outlined in [the documentation](http://cakebuild.net/docs/fundamentals/configuration)).

### Versioning

Note that since modules interact with the internals of Cake, they are tied to a specific version of Cake. The version of Cake supported by the particular module version will always be in the Release Notes of the NuGet package (and therefore also on [nuget.org](https://nuget.org/packages/Cake.BuildSystems.Module/)). Make sure to match this version number to the Cake version you're using.

## Building

Running `build.ps1` with the default target will build all system modules into `dist/modules`. Copy this to `tools/Modules` (you may need to  disable the MD5 check if using the bootstrapper) and run your script.