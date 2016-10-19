#addin "Cake.Json"
#addin "Cake.FileHelpers"

//#tool nuget:?package=GitVersion.CommandLine

public class VersionUtils
{
	private const string VersionFile = "version.json";

	public static VersionInfo LoadVersion(ICakeContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}

		VersionInfo verInfo = null;

		if (!string.IsNullOrEmpty(VersionFile) && context.FileExists(VersionFile))
		{
			verInfo = LoadVersionFromJson(context, VersionFile);
		}

		if (verInfo != null)
		{
			verInfo.CakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();
		}

		return verInfo;
	}

	private static VersionInfo LoadVersionFromJson(ICakeContext context, string versionFile)
	{
		context.Information("Loading Version Info From File: {0}", versionFile);
		if (!context.FileExists(versionFile))
		{
			context.Error("Version File Does Not Exist");
			return null;
		}

		var obj = context.DeserializeJsonFromFile<VersionInfo>(versionFile);

		return obj;
	}

	public static void UpdateVersion(ICakeContext context, VersionInfo verInfo)
	{
		if (context == null)
			throw new ArgumentNullException("context");

		if (!string.IsNullOrEmpty(VersionFile) && context.FileExists(VersionFile))
		{
			context.Information("Updating Version File {0}", VersionFile);
			context.SerializeJsonToFile(VersionFile, verInfo);
		}
	}
}

public class VersionInfo
{
	[Newtonsoft.Json.JsonProperty("major")]
	public int Major {get;set;}
	[Newtonsoft.Json.JsonProperty("minor")]
	public int Minor {get;set;}
	[Newtonsoft.Json.JsonProperty("build")]
	public int Build {get;set;}
	[Newtonsoft.Json.JsonProperty("preRelease")]
	public int? PreRelease {get;set;}
	[Newtonsoft.Json.JsonProperty("releaseNotes")]
	public string[] ReleaseNotes {get;set;}

	[Newtonsoft.Json.JsonIgnore]
	public string Semantic {get;set;}
	[Newtonsoft.Json.JsonIgnore]
	public string Milestone {get;set;}
	[Newtonsoft.Json.JsonIgnore]
	public string CakeVersion {get;set;}

	[Newtonsoft.Json.JsonIgnore]
	public bool IsPreRelease { get { return PreRelease != null && PreRelease != 0; } }

	public string ToString(bool includePreRelease = true)
	{
		var str = string.Format("{0:#0}.{1:#0}.{2:#0}", Major, Minor, Build);
		if (IsPreRelease && includePreRelease) str += string.Format("-pre{0:00}", PreRelease);

		return str;
	}

	public void Display(ICakeContext context)
	{
		context.Information("Version:");
		context.Information("\tMajor: {0}", Major);
		context.Information("\tMinor: {0}", Minor);
		context.Information("\tBuild: {0}", Build);
		context.Information("\tIs PreRelease: {0}", IsPreRelease);
		context.Information("\tPreRelease: {0}", PreRelease);
		context.Information("\tSemantic: {0}", Semantic);
		context.Information("\tMilestone: {0}", Milestone);
		context.Information("\tCake Version: {0}", CakeVersion);

		if (ReleaseNotes != null) context.Information("\tRelease Notes: {0}", ReleaseNotes);
	}
}
