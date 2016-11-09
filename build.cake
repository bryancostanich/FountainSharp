#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

#l "settingsUtils.cake"
#l "versionUtils.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./Source/bin") + Directory(configuration);
var solution = "./Source/FountainSharp.sln";
var solutionDir = System.IO.Path.GetDirectoryName(solution);

var versionInfo = VersionUtils.LoadVersion(Context);
var settings = SettingsUtils.LoadSettings(Context);

var netCoreProjects = new [] { "FountainSharp.Parse.NetStandard" };

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup((c) =>
{
	c.Information("Command Line:");
	c.Information("\tSolution: {0}", solution);
	c.Information("\tSolution directory: {0}", solutionDir);

    // Executed BEFORE the first task.
    settings.Display(c);
	versionInfo.Display(c);
});


Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information("Restoring {0}...", solution);
    // Nuget v3 restore does not work for project.json type projects on Unix-like systems

    // Restore for projects using packages.config
    NuGetRestore(solution, new NuGetRestoreSettings { Verbosity = NuGetVerbosity.Detailed });

    // Restore for projects using project.json
		Information("Restoring .NET Core projects...");
		foreach (var netCoreProject in netCoreProjects)
		{
			// Restore .NET Core projects with CLI
			var netCoreProjectPath = System.IO.Path.Combine(solutionDir, netCoreProject);
			Information($"Restoring {netCoreProject} from {netCoreProjectPath} ...");
			DotNetCoreRestore(netCoreProjectPath);
		}
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild(solution, buildSettings =>
        buildSettings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild(solution, buildSettings =>
        buildSettings.SetConfiguration(configuration));
    }

		foreach (var netCoreProject in netCoreProjects)
		{
      // Build .NET Core projects with CLI
      DotNetCoreBuild(System.IO.Path.Combine(solutionDir, netCoreProject),
			  new DotNetCoreBuildSettings { Configuration = "Release" });
		}
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
	  var fileExtensions = new[] { "exe", "dll" };
		foreach (var fileExtension in fileExtensions)
		{
    	NUnit3(solutionDir + "/**/bin/" + configuration + "/*.Tests." + fileExtension, new NUnit3Settings {
        NoResults = true
        });
		}
});

Task("Package")
    .Description("Packages all nuspec files into nupkg packages.")
//    .IsDependentOn("Build")
    .Does(() =>
{
	CreateDirectory(settings.NuGet.ArtifactsPath);

    Information("Nuspec path: " + settings.NuGet.NuSpecPath);
	var nugetProps = new Dictionary<string, string>() { {"Configuration", configuration} };
	var nuSpecFiles = GetFiles(settings.NuGet.NuSpecFileSpec);
	foreach (var nsf in nuSpecFiles)
	{
		Information("Packaging {0}", nsf);

//		if (buildSettings.NuGet.UpdateVersion)
//    {
//			VersionUtils.UpdateNuSpecVersion(Context, buildSettings, versionInfo, nsf.ToString());
//		}

	//	VersionUtils.UpdateNuSpecVersionDependency(Context, buildSettings, versionInfo, nsf.ToString());

		NuGetPack(nsf, new NuGetPackSettings {
			Version = versionInfo.ToString(),
			//ReleaseNotes = versionInfo.ReleaseNotes,
			Symbols = true,
			Properties = nugetProps,
			OutputDirectory = settings.NuGet.ArtifactsPath,
			ArgumentCustomization = args => args.Append("-NoDefaultExcludes")
		});
	}
});

Task("Publish")
    .Description("Publishes all of the nupkg packages to the nuget server. ")
    .IsDependentOn("Package")
    .Does(() =>
{
	var nupkgFiles = GetFiles(settings.NuGet.NuGetPackagesSpec);
	foreach(var pkg in nupkgFiles)
	{
		// Lets skip everything except the current version and we can skip the symbols pkg for now
		if (!pkg.ToString().Contains(versionInfo.ToString()) || pkg.ToString().Contains("symbols")) {
			Information("Skipping {0}", pkg);
			continue;
		}

		Information("Publishing {0}", pkg);

		var nugetSettings = new NuGetPushSettings
        {
			Source = settings.NuGet.FeedUrl,
			Verbosity = NuGetVerbosity.Detailed
		};

        if (FileExists(settings.NuGet.NuGetConfig))
			nugetSettings.ConfigFile = settings.NuGet.NuGetConfig;

		if (!string.IsNullOrEmpty(settings.NuGet.FeedApiKey))
		{
			nugetSettings.ApiKey = settings.NuGet.FeedApiKey;
		}

		NuGetPush(pkg, nugetSettings);
	}
});

Task("IncrementVersion")
	.Description("Increments the version number and then updates it in the necessary files")
	.Does(() =>
{
	var oldVer = versionInfo.ToString();
	if (versionInfo.IsPreRelease) versionInfo.PreRelease++; else versionInfo.Build++;

	Information("Incrementing Version {0} to {1}", oldVer, versionInfo.ToString());
	VersionUtils.UpdateVersion(Context, versionInfo);
});

Task("BuildNewVersion")
	.Description("Increments and Builds a new version")
	.IsDependentOn("IncrementVersion")
	.IsDependentOn("Build")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
{
});

Task("PublishNewVersion")
	.Description("Increments, Builds, and publishes a new version")
	.IsDependentOn("BuildNewVersion")
	.IsDependentOn("Publish")
	.Does(() =>
{
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
