#addin "Cake.Json"

public class SettingsUtils
{
	private const string SettingsFile = "settings.json";

	public static Settings LoadSettings(ICakeContext context)
	{
		context.Information("Loading Settings: {0}", SettingsFile);
		if (!context.FileExists(SettingsFile))
		{
			context.Error("Settings File Does Not Exist");
			return null;
		}

		var obj = context.DeserializeJsonFromFile<Settings>(SettingsFile);

		var envFeedUrl = context.EnvironmentVariable("NUGET_FEED_URL");
		if (!string.IsNullOrEmpty(envFeedUrl))
			obj.NuGet.FeedUrl = envFeedUrl;

		var envFeedApiKey = context.EnvironmentVariable("NUGET_FEED_APIKEY");
		if (!string.IsNullOrEmpty(envFeedApiKey))
			obj.NuGet.FeedApiKey = envFeedApiKey;

		return obj;
	}
}

public class Settings
{
	public NuGetSettings NuGet { get; set; }

	public void Display(ICakeContext context)
	{
		context.Information("Settings:");
		NuGet.Display(context);
	}
}

public class NuGetSettings
{
	public NuGetSettings()
	{
		NuSpecPath = "./nuspec";
		NuGetConfig = "./.nuget/NuGet.Config";
		ArtifactsPath = "artifacts/packages";
		UpdateVersion = false;
		VersionDependencyForLibrary = VersionDependencyTypes.none;
	}

	public string NuGetConfig {get;set;}
	// NuGet feed url 
	public string FeedUrl { get; set; }
	// API key for the NuGet feed
	public string FeedApiKey { get; set; }
	public string NuSpecPath {get;set;}
	public string ArtifactsPath {get;set;}
	public bool UpdateVersion {get;set;}
	public VersionDependencyTypes VersionDependencyForLibrary {get;set;}

	public string NuSpecFileSpec {
		get
	 	{
			return string.Format("{0}/*.nuspec", NuSpecPath);
		}
	}

	public string NuGetPackagesSpec
	{
		get
		{
			return string.Format("{0}/*.nupkg", ArtifactsPath);
		}
	}

	public void Display(ICakeContext context)
	{
		context.Information("NuGet Settings:");
		context.Information("\tNuGet Config: {0}", NuGetConfig);
		context.Information("\tFeed Url: {0}", FeedUrl);
		//context.Information("\tFeed API Key: {0}", FeedApiKey);
		context.Information("\tNuSpec Path: {0}", NuSpecPath);
		context.Information("\tNuSpec File Spec: {0}", NuSpecFileSpec);
		context.Information("\tArtifacts Path: {0}", ArtifactsPath);
		context.Information("\tNuGet Packages Spec: {0}", NuGetPackagesSpec);
		context.Information("\tUpdate Version: {0}", UpdateVersion);
		context.Information("\tForce Version Match: {0}", VersionDependencyForLibrary);
	}
}

public class VersionDependencyTypes
{
	public string Value { get; set; }
	public VersionDependencyTypes(string value)
	{
		Value = value;
	}

	public static implicit operator string(VersionDependencyTypes x) {return x.Value;}
	public static implicit operator VersionDependencyTypes(String text) {return new VersionDependencyTypes(text);}

	public static VersionDependencyTypes none = new VersionDependencyTypes("none");
	public static VersionDependencyTypes exact = new VersionDependencyTypes("exact");
	public static VersionDependencyTypes greaterthan = new VersionDependencyTypes("greaterthan");
	public static VersionDependencyTypes greaterthanorequal = new VersionDependencyTypes("greaterthanorequal");
	public static VersionDependencyTypes lessthan = new VersionDependencyTypes("lessthan");
}
