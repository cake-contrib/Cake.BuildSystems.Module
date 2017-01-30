///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./src/Cake.TFBuild.Module.sln");
var solution = ParseSolution(solutionPath);
var projects = solution.Projects;
var projectPaths = projects.Select(p => p.Path.GetDirectory());
var testAssemblies = projects.Where(p => p.Name.Contains(".Tests")).Select(p => p.Path.GetDirectory() + "/bin/" + configuration + "/" + p.Name + ".dll");
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));
GitVersion versionInfo = null;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
	versionInfo = GitVersion();
	Information("Building for version {0}", versionInfo.FullSemVer);
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	// Clean solution directories.
	foreach(var path in projectPaths)
	{
		Information("Cleaning {0}", path);
		CleanDirectories(path + "/**/bin/" + configuration);
		CleanDirectories(path + "/**/obj/" + configuration);
	}
	Information("Cleaning common files...");
	CleanDirectory(artifacts);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	NuGetRestore(solutionPath);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	MSBuild(solutionPath, settings =>
		settings.SetPlatformTarget(PlatformTarget.MSIL)
			.WithProperty("TreatWarningsAsErrors","true")
			.SetVerbosity(Verbosity.Quiet)
			.WithTarget("Build")
			.SetConfiguration(configuration));
});

Task("Post-Build")
	.IsDependentOn("Build")
	.Does(() =>
{
	CreateDirectory(artifacts + "build");
	foreach (var project in projects) {
		CreateDirectory(artifacts + "build/" + project.Name);
		CopyFiles(GetFiles(project.Path.GetDirectory() + "/" + project.Name + ".xml"), artifacts + "build/" + project.Name);
		var files = GetFiles(project.Path.GetDirectory() +"/bin/" +configuration +"/net45/" +project.Name +".*");
		CopyFiles(files, artifacts + "build/" + project.Name);
	}
});

Task("Default")
	.IsDependentOn("Post-Build");

RunTarget(target);
