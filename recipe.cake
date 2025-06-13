#load nuget:?package=Cake.Recipe&version=3.1.1

Environment.SetVariableNames();

var standardNotificationMessage = "Version {0} of {1} has just been released, this will be available here https://www.nuget.org/packages/{1}, once package indexing is complete.";

BuildParameters.SetParameters(
  context: Context,
  buildSystem: BuildSystem,
  sourceDirectoryPath: "./src",
  title: "Cake.BuildSystems.Module",
  repositoryOwner: "cake-contrib",
  shouldRunDotNetCorePack: true,
  shouldUseDeterministicBuilds: true,
  gitterMessage: "@/all " + standardNotificationMessage,
  twitterMessage: standardNotificationMessage,
  shouldRunCodecov: false,
  preferredBuildProviderType: BuildProviderType.GitHubActions,
  preferredBuildAgentOperatingSystem: PlatformFamily.Linux,
  shouldUseTargetFrameworkPath: false);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolPreprocessorDirectives(
    gitReleaseManagerGlobalTool: "#tool dotnet:?package=GitReleaseManager.Tool&version=0.18.0",
    gitVersionGlobalTool: "#tool dotnet:?package=GitVersion.Tool&version=5.12.0");

ToolSettings.SetToolSettings(context: Context);

Build.RunDotNetCore();
