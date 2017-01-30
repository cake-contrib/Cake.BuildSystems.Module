# Cake.TFBuild.Module
A simple Cake module to enhance running from a TF Build environment

Currently this module supports:

- Individual timeline records for each task
- Integrates `Warning` and `Error` logging aliases with the Build Issues summary
- Includes a Cake Build Summary widget on the build summary page

Most importantly, these enhancements will only apply while running in a TF Build environment (i.e. VSTS or TFS vNext builds), and all existing Cake behaviour remains unchanged.

Loading this module (by default, from the `tools/Modules` folder) will enable these additional features: **no changes are required to your script**.