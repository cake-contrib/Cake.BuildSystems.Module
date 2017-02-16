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

### Travis CI

> This module is in-progress and untested

- Log folding for the Cake build and for individual tasks

## Usage

Each build system's functionality resides in its own module, with `Cake.Module.Shared` used for shared types. Each module will conditionally register itself, meaning they will only be loaded in their respective CI environments. This means all modules can be deployed with a single codebase without interference.

Running `build.ps1` with the default target will build all system modules into `dist/modules`. Copy this to `tools/Modules` (and disable the MD5 check if using the bootstrapper) and run your script.