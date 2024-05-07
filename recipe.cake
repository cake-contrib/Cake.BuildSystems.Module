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
  preferredBuildAgentOperatingSystem: PlatformFamily.Linux);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

Build.RunDotNetCore();
