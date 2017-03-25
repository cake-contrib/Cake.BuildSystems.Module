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

### Using the default bootstrapper

The updated [default bootstrapper](https://github.com/cake-build/resources/tree/develop/build.ps1) can install modules when added to a `packages.config` in the `tools/Modules` directory. Add a new file like below to install this module before execution:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
    <package id="Cake.BuildSystems.Module" version="0.1.0" />
</packages>
```

### Manually

Running `build.ps1` from this repo with the default target will build all system modules into `dist/modules`. Copy this to your `tools/Modules` (and disable the MD5 check if necessary) and run your script.