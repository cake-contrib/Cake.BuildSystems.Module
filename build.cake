//#tool "nuget:?package=GitVersion.CommandLine"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./src/Cake.BuildSystems.Module.sln");
var solution = ParseSolution(solutionPath);
var projects = solution.Projects.Where(p => p.Type != "{2150E333-8FDC-42A3-9474-1A3956D46DE8}");
var projectPaths = projects.Select(p => p.Path.GetDirectory());
var testAssemblies = projects.Where(p => p.Name.Contains(".Tests")).Select(p => p.Path.GetDirectory() + "/bin/" + configuration + "/" + p.Name + ".dll");
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));
GitVersion versionInfo = null;
var frameworks = new List<string> { "net45", "netstandard1.6"};

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
	//versionInfo = GitVersion();
	//Information("Building for version {0}", versionInfo.FullSemVer);
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
	//NuGetRestore(solutionPath);
	foreach (var project in projectPaths) {
		DotNetCoreRestore(project.FullPath);
	}
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	foreach(var framework in frameworks) {
		foreach (var project in projectPaths) {
			var settings = new DotNetCoreBuildSettings {
				Framework = framework,
				Configuration = configuration,
				NoIncremental = true,
			};
			DotNetCoreBuild(project.FullPath, settings);
		}
	}
	
});

Task("Post-Build")
	.IsDependentOn("Build")
	.Does(() =>
{
	CreateDirectory(artifacts + "build");
	CreateDirectory(artifacts + "modules");
	foreach (var project in projects) {
		CreateDirectory(artifacts + "build/" + project.Name);
		//CopyFiles(GetFiles(project.Path.GetDirectory() + "/bin/" + configuration + "/net45/" + project.Name + ".xml"), artifacts + "build/" + project.Name);
		var files = GetFiles(project.Path.GetDirectory() + "/bin/" + configuration + "/net45/" + project.Name +".*");
		CopyFiles(files, artifacts + "build/" + project.Name);
		CopyFiles(files, artifacts + "modules/");
	}
});

Task("NuGet")
	.IsDependentOn("Post-Build")
	.Does(() => 
{
	CreateDirectory(artifacts + "package");
	Information("Building NuGet package");
	var versionNotes = ParseAllReleaseNotes("./ReleaseNotes.md").FirstOrDefault(v => v.Version.ToString() == versionInfo.MajorMinorPatch);
	var files = GetFiles(artifacts + "modules/*.dll") + GetFiles(artifacts + "modules/*.xml");
	var content = files.Select(f => new NuSpecContent { Source = f.FullPath, Target = "lib/netstandard1.6"}).ToList();
	var settings = new NuGetPackSettings {
		Id				= "Cake.BuildSystems.Module",
		Version			= versionInfo.NuGetVersionV2,
		Title			= "Cake Build Systems Module",
		Authors		 	= new[] { "Alistair Chapman" },
		Owners			= new[] { "achapman", "cake-contrib" },
		Description		= "This Cake module will introduce a number of features for running in hosted CI build environments to tightly integrate with the host environment/tools.",
		ReleaseNotes	= versionNotes != null ? versionNotes.Notes.ToList() : new List<string>(),
		Summary			= "A simple Cake module to enhance running from a hosted CI environment.",
		ProjectUrl		= new Uri("https://github.com/agc93/Cake.BuildSystems.Module"),
		IconUrl			= new Uri("https://cakeresources.blob.core.windows.net/nuget/64/deployment-64.png"),
		LicenseUrl		= new Uri("https://raw.githubusercontent.com/agc93/Cake.BuildSystems.Module/master/LICENSE"),
		Copyright		= "Alistair Chapman 2017",
		Tags			= new[] { "cake", "build", "ci", "build" },
		OutputDirectory = artifacts + "/package",
		Files			= content
	};

	NuGetPack(settings);
});

Task("Default")
	.IsDependentOn("Post-Build");

RunTarget(target);
